using Newtonsoft.Json.Linq;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Census;

namespace watchtower.Services.Census.Readers {

    public class CensusItemCategoryReader : ICensusReader<ItemCategory> {
        public override ItemCategory? ReadEntry(JToken token) {
            ItemCategory cat = new ItemCategory();

            cat.ID = token.GetRequiredInt32("item_category_id");

            JToken? name = token.SelectToken("name");
            cat.Name = name?.GetString("en", "<missing en name>") ?? "<missing name>";

            return cat;
        }
    }
}
