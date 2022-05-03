using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Services.Db.Patches {

    [Patch]
    public class Patch07FacilityControl : IDbPatch {

        public int MinVersion => 7;
        public string Name => "Add facility control and player control tables";

        public async Task Execute(IDbHelper helper) {
            using NpgsqlConnection conn = helper.Connection();
            using NpgsqlCommand cmd = await helper.Command(conn, @"
                CREATE TABLE IF NOT EXISTS wt_ledger (
                    id bigint NOT NULL PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
                    facility_id int NOT NULL,
                    old_faction_id smallint NOT NULL,
                    new_faction_id smallint NOT NULL,
                    outfit_id varchar NULL,
                    world_id smallint NOT NULL,
                    zone_id int NOT NULL,
                    players int NOT NULL,
                    duration_held int NOT NULL,
                    timestamp timestamptz NOT NULL DEFAULT (NOW() at time zone 'utc')
                );
            ");

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

    }
}
