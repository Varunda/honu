using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Services.Db.Patches {

    [Patch]
    public class Patch15AddLowercaseCharacterName : IDbPatch {

        public int MinVersion => 15;

        public string Name => "Add name_lower to wt_character";

        public async Task Execute(IDbHelper helper) {
            using NpgsqlConnection conn = helper.Connection();
            using NpgsqlCommand cmd = await helper.Command(conn, @"
                ALTER TABLE wt_character
	                ADD COLUMN name_lower varchar NOT NULL GENERATED ALWAYS AS (LOWER(name)) STORED;
            ");

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

    }
}
