using Npgsql;
using System.Data;
using watchtower.Models.Census;

namespace watchtower.Services.Db.Readers {

    public class ItemCategoryReader : IDataReader<ItemCategory> {

        public override ItemCategory? ReadEntry(NpgsqlDataReader reader) {
            ItemCategory cat = new ItemCategory();

            cat.ID = reader.GetInt32("id");
            cat.Name = reader.GetString("name");

            return cat;
        }

    }
}
