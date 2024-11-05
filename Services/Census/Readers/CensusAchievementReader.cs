using Newtonsoft.Json.Linq;
using System.Text.Json;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Census;
using watchtower.Services.Metrics;

namespace watchtower.Services.Census.Readers {

    public class CensusAchievementReader : ICensusReader<Achievement> {

        public CensusAchievementReader(CensusMetric metrics) : base(metrics) { }

        public override Achievement? ReadEntry(JsonElement token) {
            Achievement ach = new();

            ach.ID = token.GetRequiredInt32("achievement_id");
            ach.ItemID = token.GetValue<int?>("item_id");
            ach.ObjectiveGroupID = token.GetRequiredInt32("objective_group_id");
            ach.RewardID = token.GetValue<int?>("reward_id");
            ach.Repeatable = token.GetRequiredInt32("repeatable") == 1;
            ach.ImageID = token.GetInt32("image_id", 0);
            ach.ImageSetID = token.GetInt32("image_set_id", 0);

            ach.Name = token.GetChild("name")?.GetString("en", "<missing name>") ?? "<missing name>";
            ach.Description = token.GetChild("description")?.GetString("en", "<missing description>") ?? "<missing description>";

            return ach;
        }

    }
}
