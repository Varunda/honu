using Microsoft.Extensions.Logging;
using Npgsql;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Census;

namespace watchtower.Services.Db {

    /// <summary>
    ///     Service to interact with the alerts table
    /// </summary>
    public class AlertDbStore {

        private readonly ILogger<AlertDbStore> _Logger;
        private readonly IDbHelper _DbHelper;
        private readonly IDataReader<PsAlert> _Reader;

        public AlertDbStore(ILogger<AlertDbStore> logger, 
            IDbHelper dbHelper, IDataReader<PsAlert> reader) {

            _Logger = logger;
            _DbHelper = dbHelper;
            _Reader = reader;
        }

        /// <summary>
        ///     Get a single <see cref="PsAlert"/> by ID
        /// </summary>
        /// <param name="ID">ID of the alert</param>
        /// <returns>The <see cref="PsAlert"/> with ID of <paramref name="ID"/>, or null if it doesn't exist</returns>
        public async Task<PsAlert?> GetByID(long ID) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM alerts
                    WHERE id = @ID;
            ");

            cmd.AddParameter("ID", ID);

            PsAlert? alert = await _Reader.ReadSingle(cmd);
            await conn.CloseAsync();

            return alert;
        }

        /// <summary>
        ///     Get the alerts that haven't been finished
        /// </summary>
        /// <returns></returns>
        public async Task<List<PsAlert>> LoadUnfinished() {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM alerts
                    --WHERE NOW() AT TIME ZONE 'utc' > (timestamp + (duration || ' seconds')::INTERVAL);
                    WHERE NOW() AT TIME ZONE 'utc' < ((timestamp + (duration || ' seconds')::INTERVAL) AT TIME ZONE 'utc');
            ");

            List<PsAlert> alerts = await _Reader.ReadList(cmd);
            await conn.CloseAsync();

            return alerts;
        }

        /// <summary>
        ///     Insert a new alert using the parameter passed in from <paramref name="alert"/>
        /// </summary>
        /// <param name="alert">Parameters used to insert</param>
        /// <returns>ID of the alert that was just inserted</returns>
        public async Task<long> Insert(PsAlert alert) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                INSERT INTO alerts (
                    timestamp, duration, zone_id, world_id, alert_id, victor_faction_id, warpgate_vs, warpgate_nc, warpgate_tr, zone_facility_count, count_vs, count_nc, count_tr
                ) VALUES (
                    @Timestamp, @Duration, @ZoneID, @WorldID, @AlertID, null, @WarpgateVS, @WarpgateNC, @WarpgateTR , @ZoneFacilityCount, null, null, null
                ) RETURNING id;
            ");

            cmd.AddParameter("Timestamp", alert.Timestamp);
            cmd.AddParameter("Duration", alert.Duration);
            cmd.AddParameter("ZoneID", alert.ZoneID);
            cmd.AddParameter("WorldID", alert.WorldID);
            cmd.AddParameter("AlertID", alert.AlertID);
            cmd.AddParameter("WarpgateVS", alert.WarpgateVS);
            cmd.AddParameter("WarpgateNC", alert.WarpgateNC);
            cmd.AddParameter("WarpgateTR", alert.WarpgateTR);
            cmd.AddParameter("ZoneFacilityCount", alert.ZoneFacilityCount);

            long ID = await cmd.ExecuteInt64(CancellationToken.None);

            return ID;
        }

        /// <summary>
        ///     Update the parameters that are only known after an alert has ended
        /// </summary>
        /// <param name="ID">ID of the alert to update</param>
        /// <param name="parameters">Parameters used to update</param>
        public async Task UpdateByID(long ID, PsAlert parameters) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                UPDATE alerts
                    SET victor_faction_id = @VictorFactionID,
                        count_vs = @CountVS,
                        count_nc = @CountNC,
                        count_tr = @CountTR
                    WHERE id = @ID;
            ");

            cmd.AddParameter("ID", ID);
            cmd.AddParameter("VictorFactionID", parameters.VictorFactionID);
            cmd.AddParameter("CountVS", parameters.CountVS);
            cmd.AddParameter("CountNC", parameters.CountNC);
            cmd.AddParameter("CountTR", parameters.CountTR);

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

    }
}
