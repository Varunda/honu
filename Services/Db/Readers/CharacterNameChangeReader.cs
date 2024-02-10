using Npgsql;
using System.Data;
using watchtower.Models.Db;

namespace watchtower.Services.Db.Readers {

    public class CharacterNameChangeReader : IDataReader<CharacterNameChange> {

        public override CharacterNameChange? ReadEntry(NpgsqlDataReader reader) {
            CharacterNameChange change = new();

            change.ID = reader.GetInt64("id");
            change.CharacterID = reader.GetString("character_id");
            change.OldName = reader.GetString("old_name");
            change.NewName = reader.GetString("new_name");
            change.LowerBound = reader.GetDateTime("lower_bound");
            change.UpperBound = reader.GetDateTime("upper_bound");
            change.Timestamp = reader.GetDateTime("timestamp");

            return change;
        }

    }
}
