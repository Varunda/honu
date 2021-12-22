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

    public class DirectiveTreeCategoryDbStore {

        private readonly ILogger<DirectiveTreeCategoryDbStore> _Logger;
        private readonly IDbHelper _DbHelper;
        private readonly IDataReader<DirectiveTreeCategory> _Reader;

        public DirectiveTreeCategoryDbStore(ILogger<DirectiveTreeCategoryDbStore> logger,
                IDbHelper helper, IDataReader<DirectiveTreeCategory> reader) {

            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _DbHelper = helper ?? throw new ArgumentNullException(nameof(helper));
            _Reader = reader ?? throw new ArgumentNullException(nameof(reader));
        }

        public async Task<List<DirectiveTreeCategory>> GetAll() {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM directive_tree_category; 
            ");

            List<DirectiveTreeCategory> dirs = await _Reader.ReadList(cmd);
            await conn.CloseAsync();

            return dirs;
        }

        public async Task Upsert(DirectiveTreeCategory cat) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                INSERT INTO directive_tree_category (
                    id, name
                ) VALUES (
                    @ID, @Name
                ) ON CONFLICT (id) DO
                    UPDATE SET name = @Name;
            ");

            cmd.AddParameter("ID", cat.ID);
            cmd.AddParameter("Name", cat.Name);

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

    }
}
