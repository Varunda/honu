using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Services.Db.Patches {

    [Patch]
    public class Patch26AddPlayerControlTable : IDbPatch {

        public int MinVersion => 26;

        public string Name => "Add wt_ledger_player";

        public async Task Execute(IDbHelper helper) {
            using NpgsqlConnection conn = helper.Connection();
            using NpgsqlCommand cmd = await helper.Command(conn, @"
                CREATE TABLE IF NOT EXISTS wt_ledger_player (
                    id bigint NOT NULL PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
                    control_id bigint NOT NULL,
                    character_id varchar NOT NULL,
                    facility_id int NOT NULL,
                    outfit_id varchar NULL,
                    timestamp timestamptz NOT NULL
                );

                CREATE INDEX IF NOT EXISTS idx_wt_ledger_player_control_id ON wt_ledger_player (control_id);
            ");

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

    }
}
