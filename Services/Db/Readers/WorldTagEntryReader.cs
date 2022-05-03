using Npgsql;
using System.Data;
using watchtower.Code.ExtensionMethods;
using watchtower.Models;

namespace watchtower.Services.Db.Readers {

    public class WorldTagEntryReader : IDataReader<WorldTagEntry> {

        public override WorldTagEntry? ReadEntry(NpgsqlDataReader reader) {
            WorldTagEntry entry = new WorldTagEntry();

            entry.ID = reader.GetInt64("id");
            entry.CharacterID = reader.GetString("character_id");
            entry.WorldID = reader.GetInt16("world_id");
            entry.Timestamp = reader.GetDateTime("timestamp");
            entry.TargetKilled = reader.GetNullableDateTime("target_killed");
            entry.Kills = reader.GetInt32("kills");
            entry.WasKilled = reader.GetBoolean("was_killed");

            return entry;
        }

    }
}
