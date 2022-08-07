using Npgsql;
using System.Data;
using watchtower.Models.Report;

namespace watchtower.Services.Db.Readers {

    public class OutfitReportReader : IDataReader<OutfitReportParameters> {

        public override OutfitReportParameters? ReadEntry(NpgsqlDataReader reader) {
            OutfitReportParameters report = new();

            report.ID = reader.GetGuid("id");
            report.Generator = reader.GetString("generator");
            report.Timestamp = reader.GetDateTime("timestamp");
            report.PeriodStart = reader.GetDateTime("period_start");
            report.PeriodEnd = reader.GetDateTime("period_end");

            return report;
        }

    }
}
