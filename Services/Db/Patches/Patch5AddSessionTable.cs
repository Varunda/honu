using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Services.Db.Patches {

    [Patch]
    public class Patch5AddSessionTable : IDbPatch {

        public int MinVersion => 5;
        public string Name => "Add wt_session";

        public async Task Execute(IDbHelper helper) {
            using (NpgsqlConnection conn = helper.Connection()) {
                using NpgsqlCommand cmd = await helper.Command(conn, @"
                    CREATE TABLE IF NOT EXISTS wt_session (
                        id bigint NOT NULL PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
                        character_id varchar NOT NULL,
                        start timestamptz NOT NULL DEFAULT (NOW() at time zone 'utc'),
                        finish timestamptz NULL
                    );
                ");

                await cmd.ExecuteNonQueryAsync();
            }

            if (await helper.HasIndex("wt_session", "idx_wt_session_character_id") == false) {
                using NpgsqlConnection conn = helper.Connection();
                using NpgsqlCommand cmd = await helper.Command(conn, @"
                    CREATE INDEX idx_wt_session_character_id ON wt_session(character_id);
                ");
                await conn.CloseAsync();
            }
        }

    }
}
