using Newtonsoft.Json.Linq;
using System.Text.Json;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Census;
using watchtower.Services.Metrics;

namespace watchtower.Services.Census.Readers {

    /// <summary>
    /// Read entries from the /characters_item collection
    /// </summary>
    public class CensusCharacterItemReader : ICensusReader<CharacterItem> {
        public CensusCharacterItemReader(CensusMetric metrics) : base(metrics) {
        }

        public override CharacterItem? ReadEntry(JsonElement token) {
            CharacterItem item = new CharacterItem();

            item.CharacterID = token.GetRequiredString("character_id");
            item.ItemID = token.GetRequiredString("item_id");
            item.AccountLevel = token.GetBoolean("account_level", false); // Field only present if account unlock
            item.StackCount = token.GetValue<int?>("stack_count");

            return item;
        }

    }
}
