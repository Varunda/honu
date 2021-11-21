using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Services.Db.Patches {

    [Patch]
    public class Patch20AddMoreCharacterColumns : IDbPatch {

        public int MinVersion => 20;

        public string Name => "Add times field to wt_character";

        public async Task Execute(IDbHelper helper) {
            using NpgsqlConnection conn = helper.Connection();
            using NpgsqlCommand cmd = await helper.Command(conn, @"
                ALTER TABLE wt_character
	                ADD COLUMN IF NOT EXISTS time_create timestamptz NULL;

                ALTER TABLE wt_character
	                ADD COLUMN IF NOT EXISTS time_last_login timestamptz NULL;

                ALTER TABLE wt_character
	                ADD COLUMN IF NOT EXISTS time_last_save timestamptz NULL;
            ");

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }
    }
}
