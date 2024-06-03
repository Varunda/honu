using Npgsql;
using System.Data;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Db;

namespace watchtower.Services.Db.Readers {

    public class SmallerExpEventReader : IDataReader<SmallerExpEvent> {

        public override SmallerExpEvent? ReadEntry(NpgsqlDataReader reader) {
            SmallerExpEvent ev = new();

            ev.SourceID = ulong.Parse(reader.GetString("source_character_id"));
            ev.ExperienceID = reader.GetInt32("experience_id");
            ev.Amount = reader.GetInt32("amount");
            ev.Timestamp = reader.GetDateTime("timestamp");

            return ev;
        }

    }
}
