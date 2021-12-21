using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Services.Db.Patches {

    [Patch]
    public class Patch23AddIndexToWtCharacterNameLower : IDbPatch {

        public int MinVersion => 23;
        public string Name => "Add index to wt_character.name_lower";

        public async Task Execute(IDbHelper helper) {
            // To help make name searching fast, Honu uses this extension to add partial name indexes so indexes can be used with 'LIKE %name%'
            // https://niallburkley.com/blog/index-columns-for-like-in-postgres/
            using NpgsqlConnection conn = helper.Connection();
            using NpgsqlCommand cmd = await helper.Command(conn, @"
                CREATE EXTENSION IF NOT EXISTS pg_trgm;

                CREATE INDEX IF NOT EXISTS idx_wt_character_name_lower
                    ON wt_character USING gin (name_lower gin_trgm_ops);
            ");

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

    }
}
