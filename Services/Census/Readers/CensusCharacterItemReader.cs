using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Census;

namespace watchtower.Services.Census.Readers {

    /// <summary>
    /// Read entries from the /characters_item collection
    /// </summary>
    public class CensusCharacterItemReader : ICensusReader<CharacterItem> {

        public override CharacterItem? ReadEntry(JToken token) {
            CharacterItem item = new CharacterItem();

            item.CharacterID = token.GetRequiredString("character_id");
            item.ItemID = token.GetRequiredString("item_id");
            item.AccountLevel = token.GetBoolean("account_level", false); // Field only present if account unlock
            item.StackCount = token.Value<int?>("stack_count");

            return item;
        }

    }
}
