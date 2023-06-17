using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Census;

namespace watchtower.Services.Census.Readers {

    public class CensusDirectiveTierReader : ICensusReader<DirectiveTier> {

        public override DirectiveTier? ReadEntry(JsonElement token) {
            DirectiveTier tier = new DirectiveTier();

            tier.TreeID = token.GetRequiredInt32("directive_tree_id");
            tier.TierID = token.GetRequiredInt32("directive_tier_id");
            tier.DirectivePoints = token.GetInt32("directive_points", 0);
            tier.CompletionCount = token.GetInt32("completion_count", 0);
            tier.RewardSetID = token.GetValue<int?>("reward_set_id");
            tier.Name = token.GetChild("name")?.GetString("en", "<missing name>") ?? "<missing name>";
            tier.ImageSetID = token.GetInt32("image_set_id", 0);
            tier.ImageID = token.GetInt32("image_id", 0);

            return tier;
        }

    }
}
