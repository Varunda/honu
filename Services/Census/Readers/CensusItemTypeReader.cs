using Newtonsoft.Json.Linq;
using System.Text.Json;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Census;
using watchtower.Services.Metrics;

namespace watchtower.Services.Census.Readers {

    public class CensusItemTypeReader : ICensusReader<ItemType> {
        public CensusItemTypeReader(CensusMetric metrics) : base(metrics) {
        }

        public override ItemType? ReadEntry(JsonElement token) {
            ItemType type = new ItemType();

            type.ID = token.GetRequiredInt32("item_type_id");
            type.Name = token.GetString("name", "");
            type.Code = token.GetString("code", "");

            return type;
        }

    }
}
