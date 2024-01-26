using Npgsql;
using System.Data;
using watchtower.Models.Alert;

namespace watchtower.Services.Db.Readers.Alert {

    public class AlertPlayerReader : IDataReader<CharacterAlertPlayer> {

        public override CharacterAlertPlayer? ReadEntry(NpgsqlDataReader reader) {
            CharacterAlertPlayer part = new CharacterAlertPlayer();

            part.CharacterID = reader.GetString("character_id");

            return part;
        }

    }
}
