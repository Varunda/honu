using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Census;
using watchtower.Models.Db;

namespace watchtower.Services.Db.Readers {

    public class DirectiveTierReader : IDataReader<DirectiveTier> {

        public override DirectiveTier? ReadEntry(NpgsqlDataReader reader) {
            DirectiveTier tier = new DirectiveTier();

            tier.TreeID = reader.GetInt32("tree_id");
            tier.TierID = reader.GetInt32("tier_id");
            tier.RewardSetID = reader.GetNullableInt32("reward_set_id");
            tier.DirectivePoints = reader.GetInt32("directive_points");
            tier.CompletionCount = reader.GetInt32("completion_count");
            tier.Name = reader.GetString("name");
            tier.ImageSetID = reader.GetInt32("image_set_id");
            tier.ImageID = reader.GetInt32("image_id");

            return tier;
        }

    }
}
