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

    public class DirectiveDbStore : IStaticDbStore<PsDirective> {

        private readonly ILogger<DirectiveDbStore> _Logger;
        private readonly IDbHelper _DbHelper;
        private readonly IDataReader<PsDirective> _Reader;

        public DirectiveDbStore(ILogger<DirectiveDbStore> logger,
                IDbHelper helper, IDataReader<PsDirective> reader) {

            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _DbHelper = helper ?? throw new ArgumentNullException(nameof(helper));
            _Reader = reader ?? throw new ArgumentNullException(nameof(reader));
        }

        public async Task<List<PsDirective>> GetAll() {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM directive; 
            ");

            List<PsDirective> dirs = await _Reader.ReadList(cmd);
            await conn.CloseAsync();

            return dirs;
        }

        public async Task Upsert(PsDirective directive) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                INSERT INTO directive (
                    id, tree_id, tier_id, objective_set_id, name, description, image_set_id, image_id
                ) VALUES (
                    @ID, @TreeID, @TierID, @ObjectiveSetID, @Name, @Description, @ImageSetID, @ImageID
                ) ON CONFLICT (id) DO
                    UPDATE SET tree_id = @TreeID,
                        tier_id = @TierID,
                        objective_set_id = @ObjectiveSetID,
                        name = @Name,
                        description = @Description,
                        image_set_id = @ImageSetID,
                        image_id = @ImageID;
            ");

            cmd.AddParameter("ID", directive.ID);
            cmd.AddParameter("TreeID", directive.TreeID);
            cmd.AddParameter("TierID", directive.TierID);
            cmd.AddParameter("ObjectiveSetID", directive.ObjectiveSetID);
            cmd.AddParameter("Name", directive.Name);
            cmd.AddParameter("Description", directive.Description);
            cmd.AddParameter("ImageSetID", directive.ImageSetID);
            cmd.AddParameter("ImageID", directive.ImageID);

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

    }
}
