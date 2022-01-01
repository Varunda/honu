using Newtonsoft.Json.Linq;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Census;

namespace watchtower.Services.Census.Readers {

    public class CensusAchievementReader : ICensusReader<Achievement> {

        public override Achievement? ReadEntry(JToken token) {
            Achievement ach = new Achievement();

            ach.ID = token.GetRequiredInt32("achievement_id");
            ach.ItemID = token.Value<int?>("item_id");
            ach.ObjectiveGroupID = token.GetRequiredInt32("objective_group_id");
            ach.RewardID = token.Value<int?>("reward_id");
            ach.Repeatable = token.GetRequiredInt32("repeatable") == 1;
            ach.ImageID = token.GetInt32("image_id", 0);
            ach.ImageSetID = token.GetInt32("image_set_id", 0);

            ach.Name = token.SelectToken("name")?.GetString("en", "<missing name>") ?? "<missing name>";
            ach.Description = token.SelectToken("description")?.GetString("en", "<missing description>") ?? "<missing description>";

            return ach;
        }

    }
}
