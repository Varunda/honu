using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Report;

namespace watchtower.Services.Db.Implementations {

    public class ReportDbStore : IReportDbStore {

        private readonly ILogger<ReportDbStore> _Logger;
        private readonly IDbHelper _DbHelper;

        public ReportDbStore(ILogger<ReportDbStore> logger,
            IDbHelper dbHelper) {

            _Logger = logger;
            _DbHelper = dbHelper;
        }

        public async Task Insert(OutfitReport report) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                INSERT INTO outfit_report (
                    generator, timestamp, period_start, period_end 
                ) VALUES (
                    @Generator, NOW() at time zone 'utc', @PeriodStart, @PeriodEnd
                );
            ");

            cmd.AddParameter("Generator", report.Generator);
            cmd.AddParameter("PeriodStart", report.PeriodStart);
            cmd.AddParameter("PeriodEnd", report.PeriodEnd);

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

    }
}
