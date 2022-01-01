using Npgsql;
using System.Data;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Census;

namespace watchtower.Services.Db.Readers {

    public class AchievementReader : IDataReader<Achievement> {

        public override Achievement? ReadEntry(NpgsqlDataReader reader) {
            Achievement ach = new Achievement();

            ach.ID = reader.GetInt32("id");
            ach.ItemID = reader.GetNullableInt32("item_id");
            ach.ObjectiveGroupID = reader.GetInt32("objective_group_id");
            ach.RewardID = reader.GetNullableInt32("reward_id");
            ach.Repeatable = reader.GetBoolean("repeatable");
            ach.Name = reader.GetString("name");
            ach.Description = reader.GetString("description");
            ach.ImageID = reader.GetInt32("image_id");
            ach.ImageSetID = reader.GetInt32("image_set_id");

            return ach;
        }

    }
}
