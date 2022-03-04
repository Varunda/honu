using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Constants;
using watchtower.Models.Alert;
using watchtower.Models.Census;
using watchtower.Models.Db;
using watchtower.Models.Events;
using watchtower.Services.Db;

namespace watchtower.Services.Repositories {

    public class AlertParticipantDataRepository {

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
        /// <param name="alert"></param>
        /// <returns></returns>
        public async Task<List<AlertParticipantDataEntry>> GetByAlert(PsAlert alert) {
            List<AlertParticipantDataEntry> entries = await _ParticipantDataDb.GetByAlertID(alert.ID);

            if (entries.Count == 0) {
                entries = await GenerateAndInsertByAlert(alert);
            }

            return entries;
        }

        public async Task<List<AlertParticipantDataEntry>> GetByAlert(long alertID) {
            PsAlert? alert = await _AlertDb.GetByID(alertID);

            if (alert == null) {
                return new List<AlertParticipantDataEntry>();
            }

            return await GetByAlert(alert);
        }

        /// <summary>
        ///     Generate the alert participant data, inserting it into the DB
        /// </summary>
        /// <param name="alert"></param>
        /// <returns></returns>
        private async Task<List<AlertParticipantDataEntry>> GenerateAndInsertByAlert(PsAlert alert) {
            DateTime start = alert.Timestamp;
            DateTime end = alert.Timestamp + TimeSpan.FromSeconds(alert.Duration);

            List<KillEvent> kills = await _KillDb.GetByRange(start, end, alert.ZoneID, alert.WorldID);
            List<ExpEvent> exp = await _ExpDb.GetByRange(start, end, alert.ZoneID, alert.WorldID);
            List<AlertParticipant> parts = await _AlertDb.GetParticipants(alert);

            Dictionary<string, AlertParticipantDataEntry> data = new Dictionary<string, AlertParticipantDataEntry>();

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
            }

            foreach (KillEvent ev in kills) {
                // Skip TKs
                if (ev.KilledTeamID == ev.AttackerTeamID) {
                    continue;
                }

                if (data.TryGetValue(ev.AttackerCharacterID, out AlertParticipantDataEntry? attacker) == false) {
                    _Logger.LogWarning($"Missing attacker {ev.AttackerCharacterID}");
                } else {
                    ++attacker.Kills;
                }

                if (ev.RevivedEventID == null) {
                    if (data.TryGetValue(ev.KilledCharacterID, out AlertParticipantDataEntry? killed) == false) {
                        _Logger.LogWarning($"Missing killed {ev.KilledTeamID}");
                    } else {
                        ++killed.Deaths;
                    }
                }
            }

            foreach (ExpEvent ev in exp) {
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
            }

            List<Session> sessions = await _SessionDb.GetByRange(start, end);
            foreach (Session s in sessions) {
                if (data.TryGetValue(s.CharacterID, out AlertParticipantDataEntry? entry) == true) {
                    entry.OutfitID = s.OutfitID;
                }
            }

            List<AlertParticipantDataEntry> entries = data.Values.ToList();

            foreach (AlertParticipantDataEntry entry in entries) {
                entry.ID = await _ParticipantDataDb.Insert(entry);
            }

            return entries;
        }

    }
}
