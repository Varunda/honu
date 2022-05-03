using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Services.Db.Patches {

    [Patch]
    public class Patch03AddIndicesToKillExpTables : IDbPatch {

        public int MinVersion => 3;
        public string Name => "Add indices to wt_kill and wt_exp tables";

        public async Task Execute(IDbHelper helper) {
            if (await helper.HasIndex("wt_kills", "idx_wt_kills_world_id") == false) {
                using NpgsqlConnection conn = helper.Connection();
                using NpgsqlCommand cmd = await helper.Command(conn, @"
                    CREATE INDEX idx_wt_kills_world_id ON wt_kills (world_id)
                ");

                await cmd.ExecuteNonQueryAsync();
            }

            if (await helper.HasIndex("wt_exp", "idx_wt_exp_world_id") == false) {
                using NpgsqlConnection conn = helper.Connection();
                using NpgsqlCommand cmd = await helper.Command(conn, @"
                    CREATE INDEX idx_wt_exp_world_id ON wt_exp (world_id)
                ");

                await cmd.ExecuteNonQueryAsync();
            }
        }

    }
}
