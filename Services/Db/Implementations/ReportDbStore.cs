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

        public async Task<long> Insert(OutfitReport report) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                INSERT INTO outfit_report (
                    generator, timestamp, period_start, period_end 
                ) VALUES (
                    @Generator, NOW() at time zone 'utc', @PeriodStart, @PeriodEnd
                ) RETURNING id;
            ");

            cmd.AddParameter("Generator", report.Generator);
            cmd.AddParameter("PeriodStart", report.PeriodStart);
            cmd.AddParameter("PeriodEnd", report.PeriodEnd);

            object? objID = await cmd.ExecuteScalarAsync();
            await conn.CloseAsync();

            if (objID != null && long.TryParse(objID.ToString(), out long ID) == true) {
                return ID;
            } else {
                throw new Exception($"Missing or bad type on 'id': {objID} {objID?.GetType()}");
            }
        }

    }
}
