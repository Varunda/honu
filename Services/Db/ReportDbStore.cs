using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Report;

namespace watchtower.Services.Db {

    public class ReportDbStore {

        private readonly ILogger<ReportDbStore> _Logger;
        private readonly IDbHelper _DbHelper;
        private readonly IDataReader<OutfitReport> _ReportReader;

        public ReportDbStore(ILogger<ReportDbStore> logger,
            IDbHelper dbHelper, IDataReader<OutfitReport> reader) {

            _Logger = logger;
            _DbHelper = dbHelper;
            _ReportReader = reader ?? throw new ArgumentNullException(nameof(reader));
        }

        /// <summary>
        ///     Insert a new <see cref="OutfitReport"/> into the DB. <paramref name="report"/> may not contain a
        ///     <see cref="OutfitReport.ID"/> of <see cref="Guid.Empty"/>
        /// </summary>
        /// <param name="report">Parameters used to insert the report</param>
        public async Task Insert(OutfitReport report) {
            if (report.ID == Guid.Empty) {
                throw new ArgumentException($"The parameter {nameof(OutfitReport.ID)} may not be Guid.Empty");
            }

            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                INSERT INTO outfit_report (
                    id, generator, timestamp, period_start, period_end 
                ) VALUES (
                    @ID, @Generator, NOW() at time zone 'utc', @PeriodStart, @PeriodEnd
                ) RETURNING id;
            ");

            cmd.AddParameter("ID", report.ID);
            cmd.AddParameter("Generator", report.Generator);
            cmd.AddParameter("PeriodStart", report.PeriodStart);
            cmd.AddParameter("PeriodEnd", report.PeriodEnd);

            await cmd.ExecuteNonQueryAsync();
        }

        /// <summary>
        ///     Get a specific <see cref="OutfitReport"/> by it's <see cref="OutfitReport.ID"/>
        /// </summary>
        /// <param name="ID">ID of the report to get</param>
        /// <returns>
        ///     The <see cref="OutfitReport"/> with <see cref="OutfitReport.ID"/> of <paramref name="ID"/>,
        ///     or <c>null</c> if it doesn't exist
        /// </returns>
        public async Task<OutfitReport?> GetByID(Guid ID) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM outfit_report
                    WHERE id = @ID;
            ");

            cmd.AddParameter("ID", ID);
            OutfitReport? report = await _ReportReader.ReadSingle(cmd);

            await conn.CloseAsync();

            return report;
        }

    }
}
