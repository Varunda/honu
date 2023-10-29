using Newtonsoft.Json.Linq;
using Npgsql;
using System.Text.Json;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Census;
using watchtower.Services.Db;

namespace watchtower.Services.Census.Readers {

    public class CensusItemReader : ICensusReader<PsItem> {

        public override PsItem? ReadEntry(JsonElement token) {
            PsItem item = new PsItem();

            item.ID = token.GetRequiredInt32("item_id");
            item.TypeID = token.GetInt32("item_type_id", -1);
            item.CategoryID = token.GetInt32("item_category_id", -1);
            item.IsVehicleWeapon = token.GetInt32("is_vehicle_weapon", 0) == 1;
            item.Name = token.GetChild("name")?.GetString("en", "<missing en name>") ?? "<missing name>";
            item.Description = token.GetChild("description")?.GetString("en", "<missing en description>") ?? "<missing description>";
            item.FactionID = token.GetInt16("faction_id", -1);
            item.ImageID = token.GetInt32("image_id", 0);
            item.ImageSetID = token.GetInt32("image_set_id", 0);

            return item;
        }

    }
}
