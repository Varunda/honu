using Npgsql;
using System.Data;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Db;

namespace watchtower.Services.Db.Readers {

    public class OutfitActivityDbEntryReader : IDataReader<OutfitActivityDbEntry> {

        public override OutfitActivityDbEntry? ReadEntry(NpgsqlDataReader reader) {
            OutfitActivityDbEntry act = new();

            act.Timestamp = reader.GetDateTime("timestamp");
            act.Count = reader.GetInt32("count");

            return act;
        }

    }
}
