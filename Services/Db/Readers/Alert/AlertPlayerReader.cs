using Npgsql;
using System.Data;
using watchtower.Models.Alert;

namespace watchtower.Services.Db.Readers.Alert {

    public class AlertPlayerReader : IDataReader<AlertPlayer> {

        public override AlertPlayer? ReadEntry(NpgsqlDataReader reader) {
            AlertPlayer part = new AlertPlayer();

            part.CharacterID = reader.GetString("character_id");

            return part;
        }

    }
}
