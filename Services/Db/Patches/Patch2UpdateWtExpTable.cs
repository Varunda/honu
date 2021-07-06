using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Services.Db.Patches {

    [Patch]
    public class Patch2UpdateWtExpTable : IDbPatch {

        public int MinVersion => 2;
        public string Name => "Add source_team_id to wt_exp";

        public async Task Execute(IDbHelper helper) {
            using NpgsqlConnection conn = helper.Connection();
            using NpgsqlCommand cmd = await helper.Command(conn, @"
                ALTER TABLE wt_exp
                    ADD source_team_id smallint NOT NULL DEFAULT 0;
            ");

            await cmd.ExecuteNonQueryAsync();
        }
    }
}
