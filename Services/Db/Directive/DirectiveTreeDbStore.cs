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

    public class DirectiveTreeDbStore : IStaticDbStore<DirectiveTree> {

        private readonly ILogger<DirectiveTreeDbStore> _Logger;
        private readonly IDbHelper _DbHelper;
        private readonly IDataReader<DirectiveTree> _Reader;

        public DirectiveTreeDbStore(ILogger<DirectiveTreeDbStore> logger,
                IDbHelper helper, IDataReader<DirectiveTree> reader) {

            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _DbHelper = helper ?? throw new ArgumentNullException(nameof(helper));
            _Reader = reader ?? throw new ArgumentNullException(nameof(reader));
        }

        public async Task<List<DirectiveTree>> GetAll() {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM directive_tree; 
            ");

            List<DirectiveTree> dirs = await _Reader.ReadList(cmd);
            await conn.CloseAsync();

            return dirs;
        }

        public async Task Upsert(DirectiveTree tree) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                INSERT INTO directive_tree (
                    id, category_id, name, image_set_id, image_id
                ) VALUES (
                    @ID, @CategoryID, @Name, @ImageSetID, @ImageID
                ) ON CONFLICT (id) DO
                    UPDATE SET category_id = @CategoryID,
                        name = @Name,
                        image_set_id = @ImageSetID,
                        image_id = @ImageID;
            ");

            cmd.AddParameter("ID", tree.ID);
            cmd.AddParameter("CategoryID", tree.CategoryID);
            cmd.AddParameter("Name", tree.Name);
            cmd.AddParameter("ImageSetID", tree.ImageSetID);
            cmd.AddParameter("ImageID", tree.ImageID);

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

    }
}
