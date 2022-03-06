using Microsoft.Extensions.Logging;
using Npgsql;
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

        public AlertPlayerDataDbStore(ILogger<AlertPlayerDataDbStore> logger, IDbHelper dbHelper,
            IDataReader<AlertPlayerDataEntry> reader) {

            _Logger = logger;
            _DbHelper = dbHelper;
            _Reader = reader;
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
                    character_id, outfit_id, seconds_online,
                    kills, deaths, vehicle_kills,
                    heals, revives, shield_repairs, resupplies, spawns, repairs
                ) VALUES (
                    @AlertID,
                    @CharacterID, @OutfitID, @SecondsOnline,
                    @Kills, @Deaths, @VehicleKills,
                    @Heals, @Revives, @ShieldRepairs, @Resupplies, @Spawns, @Repairs
                ) RETURNING id;
            ");

            cmd.AddParameter("AlertID", entry.AlertID);
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

    }
}
