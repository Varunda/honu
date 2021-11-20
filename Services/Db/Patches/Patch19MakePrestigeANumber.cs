using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Services.Db.Patches {

    [Patch]
    public class Patch19MakePrestigeANumber : IDbPatch {

        public int MinVersion => 19;
        public string Name => "Make prestige an int";

        public async Task Execute(IDbHelper helper) {
            using NpgsqlConnection conn = helper.Connection();
            using NpgsqlCommand cmd = await helper.Command(conn, @"
                ALTER TABLE wt_character
	                ALTER COLUMN prestige TYPE INT USING prestige::integer;
            ");

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

    }
}
