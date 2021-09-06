using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Services.Db.Patches {

    [Patch]
    public class Patch8UnstableState : IDbPatch {

        public int MinVersion => 8;
        public string Name => "Add zone_state to wt_ledger";

        public async Task Execute(IDbHelper helper) {
            using NpgsqlConnection conn = helper.Connection();
            using NpgsqlCommand cmd = await helper.Command(conn, @"
                ALTER TABLE wt_ledger
                    ADD zone_state smallint NULL;
            ");

            await cmd.ExecuteNonQueryAsync();

            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS wt_zone_state (
                    id smallint NOT NULL,
                    name varchar NOT NULL
                );
            ";
            await cmd.ExecuteNonQueryAsync();

            cmd.CommandText = @"
                INSERT INTO wt_zone_state (id, name) VALUES
                    (0, 'locked'),
                    (1, 'single_lane'),
                    (2, 'double_lane'),
                    (3, 'unlocked');
            ";
            await cmd.ExecuteNonQueryAsync();

            await conn.CloseAsync();
        }
    }
}
