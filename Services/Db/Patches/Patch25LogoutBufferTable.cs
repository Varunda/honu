using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Services.Db.Patches {

    [Patch]
    public class Patch25LogoutBufferTable : IDbPatch {

        public int MinVersion => 25;
        public string Name => "Create logout_buffer table";

        public async Task Execute(IDbHelper helper) {
            using NpgsqlConnection conn = helper.Connection();
            using NpgsqlCommand cmd = await helper.Command(conn, @"
                CREATE TABLE IF NOT EXISTS logout_buffer (
                    character_id varchar NOT NULL PRIMARY KEY,
                    login_time timestamptz NOT NULL,
                    timestamp timestamptz NOT NULL,
                    not_found_count int NOT NULL
                );
            ");

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

    }
}
