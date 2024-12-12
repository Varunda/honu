using Npgsql;
using System.Data;
using System.Linq;
using watchtower.Models.Wrapped;

namespace watchtower.Services.Db.Readers {

    public class WrappedEntryReader : IDataReader<WrappedEntry> {

        public override WrappedEntry? ReadEntry(NpgsqlDataReader reader) {
            WrappedEntry entry = new();

            entry.ID = reader.GetGuid("id");
            entry.InputCharacterIDs = reader.GetString("input_character_ids").Split(",").ToList();
            entry.Timestamp = reader.GetDateTime("timestamp");
            entry.CreatedAt = reader.GetDateTime("created_at");
            entry.Status = reader.GetInt32("status");

            return entry;
        }

    }
}
