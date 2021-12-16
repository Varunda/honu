using Npgsql;
using System.Data;
using watchtower.Models.Report;

namespace watchtower.Services.Db.Readers {

    public class OutfitReportReader : IDataReader<OutfitReport> {

        public override OutfitReport? ReadEntry(NpgsqlDataReader reader) {
            OutfitReport report = new OutfitReport();

            report.ID = reader.GetGuid("id");
            report.Generator = reader.GetString("generator");
            report.Timestamp = reader.GetDateTime("timestamp");
            report.PeriodStart = reader.GetDateTime("period_start");
            report.PeriodEnd = reader.GetDateTime("period_end");

            return report;
        }

    }
}
