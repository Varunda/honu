using Npgsql;
using System.Data;
using watchtower.Models.Alert;

namespace watchtower.Services.Db.Readers.Alert {

    public class AlertPopulationDataReader : IDataReader<AlertPopulation> {

        public override AlertPopulation? ReadEntry(NpgsqlDataReader reader) {
            AlertPopulation pop = new AlertPopulation();

            pop.ID = reader.GetInt64("id");
            pop.AlertID = reader.GetInt64("alert_id");
            pop.Timestamp = reader.GetDateTime("timestamp");
            pop.CountVS = reader.GetInt32("count_vs");
            pop.CountNC = reader.GetInt32("count_nc");
            pop.CountTR = reader.GetInt32("count_tr");
            pop.CountUnknown = reader.GetInt32("count_unknown");

            return pop;
        }

    }

}
