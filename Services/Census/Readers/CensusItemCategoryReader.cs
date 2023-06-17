using Newtonsoft.Json.Linq;
using System.Text.Json;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Census;

namespace watchtower.Services.Census.Readers {

    public class CensusItemCategoryReader : ICensusReader<ItemCategory> {

        public override ItemCategory? ReadEntry(JsonElement token) {
            ItemCategory cat = new ItemCategory();

            cat.ID = token.GetRequiredInt32("item_category_id");

            JsonElement? name = token.GetChild("name");
            cat.Name = name?.GetString("en", "<missing en name>") ?? "<missing name>";

            return cat;
        }
    }
}
