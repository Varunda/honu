using Npgsql;
using System.Data;
using watchtower.Models.Census;

namespace watchtower.Services.Db.Readers {

    public class ItemReader : IDataReader<PsItem> {

        public override PsItem? ReadEntry(NpgsqlDataReader reader) {
            PsItem item = new PsItem();

            item.ID = reader.GetInt32("id");
            item.CategoryID = reader.GetInt32("category_id");
            item.TypeID = reader.GetInt32("type_id");
            item.IsVehicleWeapon = reader.GetBoolean("is_vehicle_weapon");
            item.Name = reader.GetString("name");
            item.Description = reader.GetString("description");
            item.FactionID = reader.GetInt16("faction_id");
            item.ImageID = reader.GetInt32("image_id");
            item.ImageSetID = reader.GetInt32("image_set_id");

            return item;
        }

    }
}
