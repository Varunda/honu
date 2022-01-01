using Microsoft.Extensions.Logging;
using Npgsql;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Census;

namespace watchtower.Services.Db {

    public class AchievementDbStore : BaseStaticDbStore<Achievement> {

        public AchievementDbStore(ILoggerFactory loggerFactory,
            IDataReader<Achievement> reader, IDbHelper helper)
            : base("achievement", loggerFactory, reader, helper) { }

        internal override void SetupUpsertCommand(NpgsqlCommand cmd, Achievement param) {
            cmd.CommandText = @"
                INSERT INTO achievement (
                    id, item_id, objective_group_id, reward_id, repeatable, name, description, image_set_id, image_id
                ) VALUES (
                    @ID, @ItemID, @ObjectiveGroupID, @RewardID, @Repeatable, @Name, @Description, @ImageSetID, @ImageID
                ) ON CONFLICT (id) DO
                    UPDATE SET item_id = @ItemID,
                        objective_group_id = @ObjectiveGroupID,
                        reward_id = @RewardID,
                        name = @Name,
                        description = @Description,
                        image_set_id = @ImageSetID,
                        image_id = @ImageID;
            ";

            cmd.AddParameter("ID", param.ID);
            cmd.AddParameter("ItemID", param.ItemID);
            cmd.AddParameter("ObjectiveGroupID", param.ObjectiveGroupID);
            cmd.AddParameter("RewardID", param.RewardID);
            cmd.AddParameter("Repeatable", param.Repeatable);
            cmd.AddParameter("Name", param.Name);
            cmd.AddParameter("Description", param.Description);
            cmd.AddParameter("ImageSetID", param.ImageSetID);
            cmd.AddParameter("ImageID", param.ImageID);
        }

    }
}
