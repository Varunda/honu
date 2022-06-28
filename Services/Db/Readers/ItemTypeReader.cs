using Npgsql;
using System.Data;
using watchtower.Models.Census;

namespace watchtower.Services.Db.Readers {

    public class ItemTypeReader : IDataReader<ItemType> {

        public override ItemType? ReadEntry(NpgsqlDataReader reader) {
            ItemType type = new ItemType();

            type.ID = reader.GetInt32("id");
            type.Name = reader.GetString("name");
            type.Code = reader.GetString("code");

            return type;
        }

    }
}
