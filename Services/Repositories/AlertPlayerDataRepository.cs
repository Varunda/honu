using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Code.Constants;
using watchtower.Constants;
using watchtower.Models.Alert;
using watchtower.Models.Census;
using watchtower.Models.Db;
using watchtower.Models.Events;
using watchtower.Services.Db;

namespace watchtower.Services.Repositories {

    public class AlertPlayerDataRepository {

        private readonly ILogger<AlertPlayerDataRepository> _Logger;

        private readonly KillEventDbStore _KillDb;
        private readonly ExpEventDbStore _ExpDb;
        private readonly AlertDbStore _AlertDb;
        private readonly SessionDbStore _SessionDb;
        private readonly AlertPlayerDataDbStore _ParticipantDataDb;
        private readonly AlertPlayerProfileDataDbStore _ProfileDataDb;

        public AlertPlayerDataRepository(ILogger<AlertPlayerDataRepository> logger,
                KillEventDbStore killDb, ExpEventDbStore expDb,
                AlertDbStore alertDb, SessionDbStore sessionDb,
                AlertPlayerDataDbStore participantDataDb, AlertPlayerProfileDataDbStore profileDataDb) {

            _Logger = logger;
            _KillDb = killDb;
            _ExpDb = expDb;
            _AlertDb = alertDb;
            _SessionDb = sessionDb;
            _ParticipantDataDb = participantDataDb;
            _ParticipantDataDb = participantDataDb;
            _ProfileDataDb = profileDataDb;
        }

        /// <summary>
        ///     Get the participant data for an alert
        /// </summary>
        /// <param name="alert">Alert to get the participation data of, generating it if necessary</param>
        /// <param name="cancel">Cancel token</param>
        /// <returns></returns>
        public async Task<List<AlertPlayerDataEntry>> GetByAlert(PsAlert alert, CancellationToken cancel) {
            List<AlertPlayerDataEntry> entries = await _ParticipantDataDb.GetByAlertID(alert.ID, cancel);

            if (entries.Count == 0) {
                entries = await GenerateAndInsertByAlert(alert);

                alert.Participants = entries.Count;
                await _AlertDb.UpdateByID(alert.ID, alert);
            }

            return entries;
        }

        /// <summary>
        ///     Get the participation data for an alert by ID
        /// </summary>
        /// <param name="alertID">ID of the alert</param>
        /// <param name="cancel">Cancel token</param>
        /// <returns>
        ///     The <see cref="AlertPlayerDataEntry"/> for the <see cref="PsAlert"/> with <see cref="PsAlert.ID"/> of <paramref name="alertID"/>,
        ///     or an empty list if the alert does not exist
        /// </returns>
        public async Task<List<AlertPlayerDataEntry>> GetByAlert(long alertID, CancellationToken cancel) {
            PsAlert? alert = await _AlertDb.GetByID(alertID);

            if (alert == null) {
                return new List<AlertPlayerDataEntry>();
            }

            return await GetByAlert(alert, cancel);
        }

        /// <summary>
        ///     Generate the alert participant data, inserting it into the DB
        /// </summary>
        /// <param name="alert"></param>
        /// <returns></returns>
        private async Task<List<AlertPlayerDataEntry>> GenerateAndInsertByAlert(PsAlert alert) {
            Stopwatch timer = Stopwatch.StartNew();

            DateTime start = alert.Timestamp;
            DateTime end = alert.Timestamp + TimeSpan.FromSeconds(alert.Duration);

            // Events not in the alert's zone are there to get when a player joins/leaves the zone
            List<KillEvent> kills = await _KillDb.GetByRange(start, end, null, alert.WorldID);
            List<ExpEvent> exp = await _ExpDb.GetByRange(start, end, null, alert.WorldID);
            List<AlertPlayer> parts = await _AlertDb.GetParticipants(alert);
            List<Session> sessions = await _SessionDb.GetByRange(start, end);

            long loadData = timer.ElapsedMilliseconds;

            Dictionary<string, AlertPlayerDataEntry> data = new Dictionary<string, AlertPlayerDataEntry>();
            Dictionary<string, List<TimestampZoneEvent>> timestampedEvents = new Dictionary<string, List<TimestampZoneEvent>>();
            Dictionary<string, List<AlertPlayerProfileData>> profileData = new Dictionary<string, List<AlertPlayerProfileData>>();

            List<string> duplicateDataEntries = new List<string>();

            foreach (AlertPlayer part in parts) {
                if (data.ContainsKey(part.CharacterID) == true) {
                    duplicateDataEntries.Add(part.CharacterID);
                    continue;
                }

                AlertPlayerDataEntry entry = new AlertPlayerDataEntry();
                entry.CharacterID = part.CharacterID;
                entry.AlertID = alert.ID;

                data.Add(entry.CharacterID, entry);

                timestampedEvents.Add(entry.CharacterID, new List<TimestampZoneEvent>());
            }

            _Logger.LogDebug($"For {alert.ID}/{alert.Name}, took {loadData}ms; have {kills.Count} kills, {exp.Count} exp events, {parts.Count} players and {sessions.Count} sessions");

            if (duplicateDataEntries.Count > 0) {
                _Logger.LogWarning($"{duplicateDataEntries.Count} duplicate entries found, skipping those");
            }

            AlertPlayerProfileData GetProfileData(string charID, short loadoutID) {
                if (profileData.TryGetValue(charID, out List<AlertPlayerProfileData>? data) == false) {
                    data = new List<AlertPlayerProfileData>();

                    profileData.Add(charID, data);
                }

                data = profileData[charID];

                foreach (AlertPlayerProfileData iter in data) {
                    if (iter.ProfileID == Profile.GetProfileID(loadoutID)) {
                        return iter;
                    }
                }

                AlertPlayerProfileData profile = new AlertPlayerProfileData();
                profile.CharacterID = charID;
                profile.ProfileID = Profile.GetProfileID(loadoutID) ?? throw new ArgumentException($"Unchecked loadoutID {loadoutID}");
                profile.AlertID = alert.ID;

                data.Add(profile);

                return profile;
            }

            int zeroCount = 0;

            foreach (KillEvent ev in kills) {
                if (ev.KilledTeamID == ev.AttackerTeamID) { // Skip TKs
                    continue;
                }

                if (timestampedEvents.TryGetValue(ev.AttackerCharacterID, out List<TimestampZoneEvent>? events) == true) {
                    events.Add(new TimestampZoneEvent() { Timestamp = ev.Timestamp, ZoneID = ev.ZoneID, LoadoutID = ev.AttackerLoadoutID, Type = "event" });
                    timestampedEvents[ev.AttackerCharacterID] = events;
                }

                if (timestampedEvents.TryGetValue(ev.KilledCharacterID, out events) == true) {
                    events.Add(new TimestampZoneEvent() { Timestamp = ev.Timestamp, ZoneID = ev.ZoneID, LoadoutID = ev.KilledLoadoutID, Type = "event" });
                    timestampedEvents[ev.KilledCharacterID] = events;
                }

                if (alert.ZoneID != 0 && ev.ZoneID != alert.ZoneID) { // Skip kills not in the alert's zone
                    continue;
                }

                if (ev.AttackerCharacterID != "0" && data.TryGetValue(ev.AttackerCharacterID, out AlertPlayerDataEntry? attacker) == true) {
                    ++attacker.Kills;

                    if (ev.AttackerLoadoutID == 0) {
                        ++zeroCount;
                        //_Logger.LogDebug($"{JToken.FromObject(ev)}");
                    } else {
                        AlertPlayerProfileData profile = GetProfileData(ev.AttackerCharacterID, ev.AttackerLoadoutID);
                        ++profile.Kills;
                    }
                }

                if (ev.KilledCharacterID != "0" && ev.RevivedEventID == null) {
                    if (data.TryGetValue(ev.KilledCharacterID, out AlertPlayerDataEntry? killed) == true) {
                        ++killed.Deaths;

                        if (ev.KilledLoadoutID != 0) {
                            AlertPlayerProfileData profile = GetProfileData(ev.KilledCharacterID, ev.KilledLoadoutID);
                            ++profile.Deaths;
                        }
                    }
                }
            }

            //_Logger.LogDebug($"zeroCount: {zeroCount}");

            foreach (ExpEvent ev in exp) {
                if (alert.ZoneID != 0 && ev.ZoneID != alert.ZoneID) { // Skip events not in the alert's zone
                    continue;
                }

                if (data.TryGetValue(ev.SourceID, out AlertPlayerDataEntry? source) == false) {
                    _Logger.LogWarning($"Missing source {ev.SourceID}");
                } else {
                    int expID = ev.ExperienceID;

                    if (Experience.IsVehicleKill(expID)) {
                        ++source.VehicleKills;

                        if (ev.LoadoutID != 0) {
                            AlertPlayerProfileData profile = GetProfileData(ev.SourceID, ev.LoadoutID);
                            ++profile.VehicleKills;
                        }
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
                    events.Add(new TimestampZoneEvent() { Timestamp = ev.Timestamp, ZoneID = ev.ZoneID, LoadoutID = ev.LoadoutID, Type = "event" });
                    timestampedEvents[ev.SourceID] = events;
                }
            }

            foreach (Session s in sessions) {
                if (data.TryGetValue(s.CharacterID, out AlertPlayerDataEntry? entry) == true) {
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
            foreach (AlertPlayer part in parts) {
                if (timestampedEvents.TryGetValue(part.CharacterID, out List<TimestampZoneEvent>? events) == false) {
                    continue;
                }
                if (data.TryGetValue(part.CharacterID, out AlertPlayerDataEntry? entry) == false) {
                    continue;
                }

                List<TimestampZoneEvent> sorted = events.OrderBy(iter => iter.Timestamp).ToList();
                if (sorted.Count <= 0) {
                    //_Logger.LogWarning($"{entry.CharacterID} has no events???");
                    continue;
                }

                TimestampZoneEvent prev = sorted[0];

                double seconds = 0d;

                foreach (TimestampZoneEvent ev in sorted.Skip(1)) {
                    if (ev.Type != "logout" && ev.Type != "login") {
                        if (alert.ZoneID == 0 || (prev.ZoneID == alert.ZoneID && ev.ZoneID == alert.ZoneID)) {
                            double sec = (ev.Timestamp - prev.Timestamp).TotalSeconds;
                            seconds += sec;

                            if (ev.LoadoutID != 0) {
                                AlertPlayerProfileData profile = GetProfileData(part.CharacterID, ev.LoadoutID);
                                profile.TimeAs += (int)sec;
                            }
                        }
                    }
                    prev = ev;
                }

                if (alert.ZoneID != 0 && prev.ZoneID == alert.ZoneID) {
                    seconds += (end - prev.Timestamp).TotalSeconds;
                }

                entry.SecondsOnline = (int)seconds;
                data[part.CharacterID] = entry;

                //_Logger.LogInformation($"Have {sorted.Count} entries to find time on continent, {seconds} seconds");
            }

            List<AlertPlayerDataEntry> entries = data.Values.ToList();

            foreach (AlertPlayerDataEntry entry in entries) {
                entry.ID = await _ParticipantDataDb.Insert(entry);

                if (profileData.TryGetValue(entry.CharacterID, out List<AlertPlayerProfileData>? profiles) == true) {
                    foreach (AlertPlayerProfileData profile in profiles) {
                        profile.ID = await _ProfileDataDb.Insert(alert.ID, profile);
                    }
                }
            }

            _Logger.LogDebug($"{alert.ID}/{alert.Name} took {timer.ElapsedMilliseconds}ms to build alert data");

            return entries;
        }

    }
}
