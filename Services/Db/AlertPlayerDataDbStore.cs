using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Alert;

namespace watchtower.Services.Db {

    /// <summary>
    ///     Service to interact with the alert_participant_data table
    /// </summary>
    public class AlertPlayerDataDbStore {

        private readonly ILogger<AlertPlayerDataDbStore> _Logger;
        private readonly IDbHelper _DbHelper;
        private readonly IDataReader<AlertPlayerDataEntry> _Reader;
        private readonly IDataReader<CharacterAlertPlayer> _CharReader;

        public AlertPlayerDataDbStore(ILogger<AlertPlayerDataDbStore> logger, IDbHelper dbHelper,
            IDataReader<AlertPlayerDataEntry> reader, IDataReader<CharacterAlertPlayer> charReader) {

            _Logger = logger;
            _DbHelper = dbHelper;
            _Reader = reader;
            _CharReader = charReader;
        }

        /// <summary>
        ///     Insert a new <see cref="AlertPlayerDataEntry"/>, returning the ID inserted
        /// </summary>
        /// <param name="entry">Parameters used to insert the data</param>
        /// <returns>The ID of the database table row just inserted</returns>
        public async Task<long> Insert(AlertPlayerDataEntry entry) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                INSERT INTO alert_participant_data (
                    alert_id,
                    character_id, outfit_id, seconds_online, timestamp,
                    kills, deaths, vehicle_kills,
                    heals, revives, shield_repairs, resupplies, spawns, repairs
                ) VALUES (
                    @AlertID,
                    @CharacterID, @OutfitID, @SecondsOnline, @Timestamp,
                    @Kills, @Deaths, @VehicleKills,
                    @Heals, @Revives, @ShieldRepairs, @Resupplies, @Spawns, @Repairs
                ) RETURNING id;
            ");

            cmd.AddParameter("AlertID", entry.AlertID);
            cmd.AddParameter("Timestamp", entry.Timestamp);
            cmd.AddParameter("CharacterID", entry.CharacterID);
            cmd.AddParameter("OutfitID", entry.OutfitID);
            cmd.AddParameter("SecondsOnline", entry.SecondsOnline);
            cmd.AddParameter("Kills", entry.Kills);
            cmd.AddParameter("Deaths", entry.Deaths);
            cmd.AddParameter("VehicleKills", entry.VehicleKills);
            cmd.AddParameter("Heals", entry.Heals);
            cmd.AddParameter("Revives", entry.Revives);
            cmd.AddParameter("ShieldRepairs", entry.ShieldRepairs);
            cmd.AddParameter("Resupplies", entry.Resupplies);
            cmd.AddParameter("Spawns", entry.Spawns);
            cmd.AddParameter("Repairs", entry.Repairs);

            long ID = await cmd.ExecuteInt64(CancellationToken.None);

            return ID;
        }

        /// <summary>
        ///     Get the participant data of an alert
        /// </summary>
        /// <param name="alertID">ID of the alert</param>
        /// <param name="cancel">Cancel token</param>
        /// <returns>
        ///     All <see cref="AlertPlayerDataEntry"/>s with <see cref="AlertPlayerDataEntry.AlertID"/> of <paramref name="alertID"/>
        /// </returns>
        public async Task<List<AlertPlayerDataEntry>> GetByAlertID(long alertID, CancellationToken cancel) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM alert_participant_data
                    WHERE alert_id = @AlertID
            ");

            cmd.AddParameter("AlertID", alertID);

            List<AlertPlayerDataEntry> entries = await _Reader.ReadList(cmd, cancel);
            await conn.CloseAsync();

            return entries;
        }

        /// <summary>
        ///     Get the daily alerts a character has participated in within a time span
        /// </summary>
        /// <param name="charID">ID of the character</param>
        /// <param name="start">Start range</param>
        /// <param name="end">End range</param>
        /// <returns>
        ///     A list of <see cref="AlertPlayerDataEntry"/>s that represent the fake daily alerts created
        /// </returns>
        public async Task<List<AlertPlayerDataEntry>> GetDailyByCharacterIDAndTimestamp(string charID, DateTime start, DateTime end) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT apd.*
                    FROM alert_participant_data apd
                        LEFT JOIN alerts a ON a.id = apd.alert_id
                    WHERE character_id = @CharacterID
                        AND apd.timestamp BETWEEN @Start AND @End
                        AND a.zone_id = 0;
            ");

            cmd.AddParameter("CharacterID", charID);
            cmd.AddParameter("Start", start);
            cmd.AddParameter("End", end);

            List<AlertPlayerDataEntry> entries = await _Reader.ReadList(cmd);
            await conn.CloseAsync();

            return entries;
        }

        /// <summary>
        ///     Delete the participant data for a specific alert
        /// </summary>
        /// <param name="alertID">ID of the alert to delete the participant data of</param>
        public async Task DeleteByAlertID(long alertID) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                DELETE
                    FROM alert_participant_data
                    WHERE alert_id = @AlertID;

                UPDATE alerts
                    SET participants = 0
                    WHERE id = @AlertID;
            ");

            cmd.AddParameter("AlertID", alertID);

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

        /// <summary>
        ///     Get the character level data of an alert a character participated in
        /// </summary>
        /// <param name="charID">ID of the character</param>
        /// <returns></returns>
        public async Task<List<CharacterAlertPlayer>> GetByCharacterID(string charID) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT p.*, a.duration, a.zone_id, a.world_id, a.victor_faction_id, a.participants, a.alert_id AS metagame_alert_id, a.instance_id, a.name
                    FROM alert_participant_data p 
                        LEFT JOIN alerts a ON p.alert_id = a.id
                    WHERE character_id = @CharacterID;
            ");

            cmd.AddParameter("CharacterID", charID);

            List<CharacterAlertPlayer> cap = await _CharReader.ReadList(cmd);
            await conn.CloseAsync();

            return cap;
        }

    }
}
