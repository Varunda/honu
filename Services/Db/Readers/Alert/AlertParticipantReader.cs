using Npgsql;
using System.Data;
using watchtower.Models.Alert;

namespace watchtower.Services.Db.Readers.Alert {

    public class AlertParticipantReader : IDataReader<AlertParticipant> {

        public override AlertParticipant? ReadEntry(NpgsqlDataReader reader) {
            AlertParticipant part = new AlertParticipant();

            part.CharacterID = reader.GetString("character_id");

            return part;
        }

    }
}
