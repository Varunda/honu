using Npgsql;
using System.Data;
using watchtower.Models.Census;

namespace watchtower.Services.Db.Readers {

    public class MetagameEventReader : IDataReader<PsMetagameEvent> {

        public override PsMetagameEvent? ReadEntry(NpgsqlDataReader reader) {
            PsMetagameEvent ev = new();

            ev.ID = reader.GetInt32("id");
            ev.Name = reader.GetString("name");
            ev.Description = reader.GetString("description");
            ev.TypeID = reader.GetInt32("type_id");

            return ev;
        }

    }
}
