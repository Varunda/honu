using Npgsql;
using System;
using System.Threading.Tasks;

namespace watchtower.Services.Db.Patches {

    [Patch]
    public class Patch69PsbAccountUpdate : IDbPatch {
        public int MinVersion => 69;
        public string Name => "update psb_named";

        public async Task Execute(IDbHelper helper) {
            using NpgsqlConnection conn = helper.Connection();
            using NpgsqlCommand cmd = await helper.Command(conn, @"
                ALTER TABLE psb_named
                    ADD COLUMN IF NOT EXISTS play_time int NOT NULL DEFAULT 0;

                ALTER TABLE psb_named
                    ADD COLUMN IF NOT EXISTS account_type smallint NOT NULL DEFAULT 0;
            ");

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }
    }
}
