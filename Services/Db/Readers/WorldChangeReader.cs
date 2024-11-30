using Npgsql;
using System.Data;
using watchtower.Models.Db;

namespace watchtower.Services.Db.Readers {

    public class WorldChangeReader : IDataReader<WorldChange> {

        public override WorldChange? ReadEntry(NpgsqlDataReader reader) {
            WorldChange change = new();

            change.CharacterID = reader.GetString("character_id");
            change.WorldID = reader.GetInt16("world_id");
            change.Timestamp = reader.GetDateTime("timestamp");

            return change;
        }

    }
}
