using Npgsql;
using System.Data;
using watchtower.Models.Api;

namespace watchtower.Services.Db.Readers {

    public class NameFightEntryReader : IDataReader<NameFightEntry> {

        public override NameFightEntry? ReadEntry(NpgsqlDataReader reader) {
            NameFightEntry entry = new();

            entry.Timestamp = reader.GetDateTime("timestamp");
            entry.WorldID = reader.GetInt16("world_id");
            entry.Total = reader.GetInt32("total");
            entry.WinsVs = reader.GetInt32("wins_vs");
            entry.WinsNc = reader.GetInt32("wins_nc");
            entry.WinsTr = reader.GetInt32("wins_tr");

            return entry;
        }

    }
}
