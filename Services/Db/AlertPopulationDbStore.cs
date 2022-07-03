using Microsoft.Extensions.Logging;
using Npgsql;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Alert;

namespace watchtower.Services.Db {

    public class AlertPopulationDbStore {

        private readonly ILogger<AlertPopulationDbStore> _Logger;
        private readonly IDbHelper _DbHelper;
        private readonly IDataReader<AlertPopulation> _Reader;

        public AlertPopulationDbStore(ILogger<AlertPopulationDbStore> logger,
            IDbHelper dbHelper, IDataReader<AlertPopulation> reader) {

            _Logger = logger;
            _DbHelper = dbHelper;
            _Reader = reader;
        }

        /// <summary>
        ///     Get the <see cref="AlertPopulation"/>s of an alert
        /// </summary>
        /// <param name="alertID">ID of the alert</param>
        /// <param name="cancel">Cancellation token</param>
        /// <returns>
        ///     A list of <see cref="AlertPopulation"/> with <see cref="AlertPopulation.AlertID"/> of <paramref name="alertID"/>,
        ///     or an empty list if not data exists
        /// </returns>
        public async Task<List<AlertPopulation>> GetByAlertID(long alertID, CancellationToken cancel) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM alert_population
                    WHERE alert_id = @AlertID;
            ");

            cmd.AddParameter("AlertID", alertID);

            List<AlertPopulation> list = await _Reader.ReadList(cmd, cancel);
            await conn.CloseAsync();

            return list;
        }

        /// <summary>
        ///     Insert a new <see cref="AlertPopulation"/> to the database
        /// </summary>
        /// <param name="alertID">ID of the alert</param>
        /// <param name="param">Parameters used to insert the value</param>
        /// <param name="cancel">Cancellation token</param>
        /// <returns>
        ///     The ID of the <see cref="AlertPopulation"/> that was just inserted
        /// </returns>
        public async Task<long> Insert(long alertID, AlertPopulation param, CancellationToken cancel) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                INSERT INTO alert_population (
                    alert_id, timestamp, count_vs, count_nc, count_tr, count_unknown
                ) VALUES (
                    @AlertID, @Timestamp, @CountVS, @CountNC, @CountTR, @CountUnknown
                ) RETURNING id;
            ");

            cmd.AddParameter("AlertID", alertID);
            cmd.AddParameter("Timestamp", param.Timestamp);
            cmd.AddParameter("CountVS", param.CountVS);
            cmd.AddParameter("CountNC", param.CountNC);
            cmd.AddParameter("CountTR", param.CountTR);
            cmd.AddParameter("CountUnknown", param.CountUnknown);

            long ID = await cmd.ExecuteInt64(cancel);

            return ID;
        }

        /// <summary>
        ///     Delete the alert population for an alert
        /// </summary>
        /// <param name="alertID">ID of the alert</param>
        /// <returns></returns>
        public async Task DeleteByAlertID(long alertID) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                DELETE
                    FROM alert_population
                    WHERE alert_id = @AlertID;
            ");

            cmd.AddParameter("AlertID", alertID);

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

    }
}
