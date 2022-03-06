using Microsoft.Extensions.Logging;
using Npgsql;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Alert;

namespace watchtower.Services.Db {

    public class AlertPlayerProfileDataDbStore {

        private readonly ILogger<AlertPlayerProfileDataDbStore> _Logger;
        private readonly IDataReader<AlertPlayerProfileData> _Reader;
        private readonly IDbHelper _DbHelper;

        public AlertPlayerProfileDataDbStore(ILogger<AlertPlayerProfileDataDbStore> logger,
            IDataReader<AlertPlayerProfileData> reader, IDbHelper dbHelper) {

            _Logger = logger;
            _Reader = reader;
            _DbHelper = dbHelper;
        }

        /// <summary>
        ///     Insert a new <see cref="AlertPlayerProfileData"/> into the table
        /// </summary>
        /// <param name="alertID">ID of the alert this data is for</param>
        /// <param name="data">Parameters used to insert the data</param>
        /// <returns>The ID of the <see cref="AlertPlayerProfileData"/> that was just inserted into the DB</returns>
        public async Task<long> Insert(long alertID, AlertPlayerProfileData data) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                INSERT INTO alert_player_profile_data (
                    alert_id, character_id, profile_id, kills, vehicle_kills, deaths, time_as
                ) VALUES (
                    @AlertID, @CharacterID, @ProfileID, @Kills, @VehicleKills, @Deaths, @TimeAs
                ) RETURNING id;
            ");

            cmd.AddParameter("AlertID", alertID);
            cmd.AddParameter("CharacterID", data.CharacterID);
            cmd.AddParameter("ProfileID", data.ProfileID);
            cmd.AddParameter("Kills", data.Kills);
            cmd.AddParameter("VehicleKills", data.VehicleKills);
            cmd.AddParameter("Deaths", data.Deaths);
            cmd.AddParameter("TimeAs", data.TimeAs);

            long ID = await cmd.ExecuteInt64(CancellationToken.None);

            return ID;
        }

        /// <summary>
        ///     Get the alert player profile data for a specific alert
        /// </summary>
        /// <param name="alertID">ID of the alert</param>
        /// <returns>A list of <see cref="AlertPlayerProfileData"/> with <see cref="AlertPlayerProfileData.AlertID"/> of <paramref name="alertID"/></returns>
        public async Task<List<AlertPlayerProfileData>> GetByAlertID(long alertID) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM alert_player_profile_data
                    WHERE alert_id = @AlertID;
            ");

            cmd.AddParameter("AlertID", alertID);

            List<AlertPlayerProfileData> data = await _Reader.ReadList(cmd);
            await conn.CloseAsync();

            return data;
        }

        /// <summary>
        ///     Delete the <see cref="AlertPlayerProfileData"/> for an alert. Useful when regenerating this data
        /// </summary>
        /// <param name="alertID">ID of the alert to wipe the data of</param>
        public async Task DeleteByAlertID(long alertID) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                DELETE
                    FROM alert_player_profile_data
                    WHERE alert_id = @AlertID;
            ");

            cmd.AddParameter("AlertID", alertID);

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

    }
}
