using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Census;

namespace watchtower.Services.Db {

    public class DirectiveTierDbStore : IStaticDbStore<DirectiveTier> {

        private readonly ILogger<DirectiveTierDbStore> _Logger;
        private readonly IDbHelper _DbHelper;
        private readonly IDataReader<DirectiveTier> _Reader;

        public DirectiveTierDbStore(ILogger<DirectiveTierDbStore> logger,
                IDbHelper helper, IDataReader<DirectiveTier> reader) {

            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _DbHelper = helper ?? throw new ArgumentNullException(nameof(helper));
            _Reader = reader ?? throw new ArgumentNullException(nameof(reader));
        }

        public async Task<List<DirectiveTier>> GetAll() {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM directive_tier; 
            ");

            List<DirectiveTier> dirs = await _Reader.ReadList(cmd);
            await conn.CloseAsync();

            return dirs;
        }

        public async Task Upsert(DirectiveTier tier) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                INSERT INTO directive_tier (
                    tier_id, tree_id, reward_set_id, directive_points, completion_count, name, image_set_id, image_id
                ) VALUES (
                    @TierID, @TreeID, @RewardSetID, @DirectivePoints, @CompletionCount, @Name, @ImageSetID, @ImageID
                ) ON CONFLICT (tier_id, tree_id) DO
                    UPDATE SET reward_set_id = @RewardSetID,
                        directive_points = @DirectivePoints,
                        completion_count = @CompletionCount,
                        name = @Name,
                        image_set_id = @ImageSetID,
                        image_id = @ImageID;
            ");

            cmd.AddParameter("TierID", tier.TierID);
            cmd.AddParameter("TreeID", tier.TreeID);
            cmd.AddParameter("RewardSetID", tier.RewardSetID);
            cmd.AddParameter("DirectivePoints", tier.DirectivePoints);
            cmd.AddParameter("CompletionCount", tier.CompletionCount);
            cmd.AddParameter("Name", tier.Name);
            cmd.AddParameter("ImageSetID", tier.ImageSetID);
            cmd.AddParameter("ImageID", tier.ImageID);

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

    }
}
