using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Report;

namespace watchtower.Services.Db {

    /// <summary>
    ///     Service to interact with the outfit_report table, which confusingly doesn't store
    ///     <see cref="OutfitReport"/>s, but rather <see cref="OutfitReportParameters"/>
    /// </summary>
    public class OutfitReportParameterDbStore {

        private readonly ILogger<OutfitReportParameterDbStore> _Logger;
        private readonly IDbHelper _DbHelper;
        private readonly IDataReader<OutfitReportParameters> _ReportReader;

        public OutfitReportParameterDbStore(ILogger<OutfitReportParameterDbStore> logger,
            IDbHelper dbHelper, IDataReader<OutfitReportParameters> reader) {

            _Logger = logger;
            _DbHelper = dbHelper;
            _ReportReader = reader;
        }

        /// <summary>
        ///     Insert a new <see cref="OutfitReport"/> into the DB. <paramref name="parms"/> may not contain a
        ///     <see cref="OutfitReportParameters.ID"/> of <see cref="Guid.Empty"/>
        /// </summary>
        /// <param name="parms">Parameters used to insert the report</param>
        public async Task Insert(OutfitReportParameters parms) {
            if (parms.ID == Guid.Empty) {
                throw new ArgumentException($"The parameter {nameof(OutfitReportParameters.ID)} may not be Guid.Empty");
            }

            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                INSERT INTO outfit_report (
                    id, generator, timestamp, period_start, period_end 
                ) VALUES (
                    @ID, @Generator, NOW() at time zone 'utc', @PeriodStart, @PeriodEnd
                ) RETURNING id;
            ");

            cmd.AddParameter("ID", parms.ID);
            cmd.AddParameter("Generator", parms.Generator);
            cmd.AddParameter("PeriodStart", parms.PeriodStart);
            cmd.AddParameter("PeriodEnd", parms.PeriodEnd);

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

        /// <summary>
        ///     Get a specific <see cref="OutfitReportParameters"/> by it's <see cref="OutfitReportParameters.ID"/>
        /// </summary>
        /// <param name="ID">ID of the report to get</param>
        /// <returns>
        ///     The <see cref="OutfitReport"/> with <see cref="OutfitReportParameters.ID"/> of <paramref name="ID"/>,
        ///     or <c>null</c> if it doesn't exist
        /// </returns>
        public async Task<OutfitReportParameters?> GetByID(Guid ID) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM outfit_report
                    WHERE id = @ID;
            ");

            cmd.AddParameter("ID", ID);
            OutfitReportParameters? report = await _ReportReader.ReadSingle(cmd);

            await conn.CloseAsync();

            return report;
        }

    }
}
