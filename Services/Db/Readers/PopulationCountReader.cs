using Npgsql;
using System.Data;
using watchtower.Models.Db;

namespace watchtower.Services.Db.Readers {

    public class PopulationCountReader : IDataReader<PopulationCount> {

        public override PopulationCount? ReadEntry(NpgsqlDataReader reader) {
            PopulationCount count = new();

            count.Timestamp = reader.GetDateTime("timestamp");
            count.Count = reader.GetInt32("count");

            return count;
        }

    }

}
