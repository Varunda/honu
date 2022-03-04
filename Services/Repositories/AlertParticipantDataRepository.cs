using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Constants;
using watchtower.Models.Alert;
using watchtower.Models.Census;
using watchtower.Models.Db;
using watchtower.Models.Events;
using watchtower.Services.Db;

namespace watchtower.Services.Repositories {

    public class AlertParticipantDataRepository {

        private class TimestampZoneEvent {
            public DateTime Timestamp;
            public uint ZoneID;
            public string Type;
        }

        private readonly ILogger<AlertParticipantDataRepository> _Logger;

        private readonly KillEventDbStore _KillDb;
        private readonly ExpEventDbStore _ExpDb;
        private readonly AlertDbStore _AlertDb;
        private readonly SessionDbStore _SessionDb;
        private readonly AlertParticipantDataDbStore _ParticipantDataDb;

        public AlertParticipantDataRepository(ILogger<AlertParticipantDataRepository> logger,
                KillEventDbStore killDb, ExpEventDbStore expDb,
                AlertDbStore alertDb, SessionDbStore sessionDb,
                AlertParticipantDataDbStore participantDataDb) {

            _Logger = logger;
            _KillDb = killDb;
            _ExpDb = expDb;
            _AlertDb = alertDb;
            _SessionDb = sessionDb;
            _ParticipantDataDb = participantDataDb;
            _ParticipantDataDb = participantDataDb;
        }

        /// <summary>
        ///     Get the participant data for an alert
        /// </summary>
        /// <param name="alert">Alert to get the participation data of, generating it if necessary</param>
        /// <param name="cancel">Cancel token</param>
        /// <returns></returns>
        public async Task<List<AlertParticipantDataEntry>> GetByAlert(PsAlert alert, CancellationToken cancel) {
            List<AlertParticipantDataEntry> entries = await _ParticipantDataDb.GetByAlertID(alert.ID, cancel);

            if (entries.Count == 0) {
                entries = await GenerateAndInsertByAlert(alert);
            }

            return entries;
        }

        /// <summary>
        ///     Get the participation data for an alert by ID
        /// </summary>
        /// <param name="alertID">ID of the alert</param>
        /// <param name="cancel">Cancel token</param>
        /// <returns></returns>
        public async Task<List<AlertParticipantDataEntry>> GetByAlert(long alertID, CancellationToken cancel) {
            PsAlert? alert = await _AlertDb.GetByID(alertID);

            if (alert == null) {
                return new List<AlertParticipantDataEntry>();
            }

            return await GetByAlert(alert, cancel);
        }

        /// <summary>
        ///     Generate the alert participant data, inserting it into the DB
        /// </summary>
        /// <param name="alert"></param>
        /// <returns></returns>
        private async Task<List<AlertParticipantDataEntry>> GenerateAndInsertByAlert(PsAlert alert) {
            DateTime start = alert.Timestamp;
            DateTime end = alert.Timestamp + TimeSpan.FromSeconds(alert.Duration);

            List<KillEvent> kills = await _KillDb.GetByRange(start, end, null, alert.WorldID);
            List<ExpEvent> exp = await _ExpDb.GetByRange(start, end, null, alert.WorldID);
            List<AlertParticipant> parts = await _AlertDb.GetParticipants(alert);

            Dictionary<string, AlertParticipantDataEntry> data = new Dictionary<string, AlertParticipantDataEntry>();
            Dictionary<string, List<TimestampZoneEvent>> timestampedEvents = new Dictionary<string, List<TimestampZoneEvent>>();

            List<string> duplicateDataEntries = new List<string>();

            foreach (AlertParticipant part in parts) {
                if (data.ContainsKey(part.CharacterID) == true) {
                    duplicateDataEntries.Add(part.CharacterID);
                    continue;
                }

                AlertParticipantDataEntry entry = new AlertParticipantDataEntry();
                entry.CharacterID = part.CharacterID;
                entry.SecondsOnline = part.SecondsOnline;
                entry.AlertID = alert.ID;

                data.Add(entry.CharacterID, entry);

                timestampedEvents.Add(entry.CharacterID, new List<TimestampZoneEvent>());
            }

            if (duplicateDataEntries.Count > 0) {
                _Logger.LogWarning($"{duplicateDataEntries.Count} duplicate entries found, skipping those");
            }

            foreach (KillEvent ev in kills) {
                if (ev.KilledTeamID == ev.AttackerTeamID) { // Skip TKs
                    continue;
                }
                if (ev.ZoneID != alert.ZoneID) { // Skip kills not in the alert's zone
                    continue;
                }

                if (data.TryGetValue(ev.AttackerCharacterID, out AlertParticipantDataEntry? attacker) == true) {
                    ++attacker.Kills;
                }

                if (ev.RevivedEventID == null) {
                    if (data.TryGetValue(ev.KilledCharacterID, out AlertParticipantDataEntry? killed) == true) {
                        ++killed.Deaths;
                    }
                }

                if (timestampedEvents.TryGetValue(ev.AttackerCharacterID, out List<TimestampZoneEvent>? events) == true) {
                    events.Add(new TimestampZoneEvent() { Timestamp = ev.Timestamp, ZoneID = ev.ZoneID, Type = "event" });
                    timestampedEvents[ev.AttackerCharacterID] = events;
                }

                if (timestampedEvents.TryGetValue(ev.KilledCharacterID, out events) == true) {
                    events.Add(new TimestampZoneEvent() { Timestamp = ev.Timestamp, ZoneID = ev.ZoneID, Type = "event" });
                    timestampedEvents[ev.KilledCharacterID] = events;
                }
            }

            foreach (ExpEvent ev in exp) {
                if (ev.ZoneID != alert.ZoneID) { // Skip events not in the alert's zone
                    continue;
                }

                if (data.TryGetValue(ev.SourceID, out AlertParticipantDataEntry? source) == false) {
                    _Logger.LogWarning($"Missing source {ev.SourceID}");
                } else {
                    int expID = ev.ExperienceID;

                    if (Experience.IsVehicleKill(expID)) {
                        ++source.VehicleKills;
                    } else if (Experience.IsHeal(expID)) {
                        ++source.Heals;
                    } else if (Experience.IsRevive(expID)) {
                        ++source.Revives;
                    } else if (Experience.IsShieldRepair(expID)) {
                        ++source.ShieldRepairs;
                    } else if (Experience.IsResupply(expID)) {
                        ++source.Resupplies;
                    } else if (Experience.IsMaxRepair(expID)) {
                        ++source.Repairs;
                    } else if (Experience.IsSpawn(expID)) {
                        ++source.Spawns;
                    }
                }

                if (timestampedEvents.TryGetValue(ev.SourceID, out List<TimestampZoneEvent>? events) == true) {
                    events.Add(new TimestampZoneEvent() { Timestamp = ev.Timestamp, ZoneID = ev.ZoneID, Type = "event" });
                    timestampedEvents[ev.SourceID] = events;
                }
            }

            List<Session> sessions = await _SessionDb.GetByRange(start, end);
            foreach (Session s in sessions) {
                if (data.TryGetValue(s.CharacterID, out AlertParticipantDataEntry? entry) == true) {
                    entry.OutfitID = s.OutfitID;

                    if (timestampedEvents.TryGetValue(s.CharacterID, out List<TimestampZoneEvent>? events) == true) {
                        events.Add(new TimestampZoneEvent() { Timestamp = s.Start, ZoneID = 0, Type = "login" });

                        if (s.End != null) {
                            events.Add(new TimestampZoneEvent() { Timestamp = s.End.Value, ZoneID = 0, Type = "logout" });
                        }
                    }
                }
            }

            // Get how long each participant was in the zone for
            foreach (AlertParticipant part in parts) {
                if (timestampedEvents.TryGetValue(part.CharacterID, out List<TimestampZoneEvent>? events) == false) {
                    continue;
                }
                if (data.TryGetValue(part.CharacterID, out AlertParticipantDataEntry? entry) == false) {
                    continue;
                }

                List<TimestampZoneEvent> sorted = events.OrderBy(iter => iter.Timestamp).ToList();
                if (sorted.Count < 2) {
                    //_Logger.LogWarning($"{entry.CharacterID} has no events???");
                    continue;
                }

                TimestampZoneEvent prev = sorted[0];

                double seconds = 0d;

                foreach (TimestampZoneEvent ev in sorted.Skip(1)) {
                    if (ev.Type != "logout") {
                        if (prev.ZoneID == alert.ZoneID && ev.ZoneID == alert.ZoneID) {
                            seconds += (ev.Timestamp - prev.Timestamp).TotalSeconds;
                        }
                    }
                    prev = ev;
                }

                if (prev.ZoneID == alert.ZoneID) {
                    seconds += (end - prev.Timestamp).TotalSeconds;
                }

                entry.SecondsOnline = (int)seconds;
                data[part.CharacterID] = entry;

                //_Logger.LogInformation($"Have {sorted.Count} entries to find time on continent, {seconds} seconds");
            }

            List<AlertParticipantDataEntry> entries = data.Values.ToList();

            foreach (AlertParticipantDataEntry entry in entries) {
                entry.ID = await _ParticipantDataDb.Insert(entry);
            }

            return entries;
        }

    }
}
