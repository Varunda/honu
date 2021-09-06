using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Services.Db.Patches {

    [Patch]
    public class Patch9FacilityTable : IDbPatch {

        public int MinVersion => 9;
        public string Name => "Add the facility table";

        public async Task Execute(IDbHelper helper) {
            using NpgsqlConnection conn = helper.Connection();
            using NpgsqlCommand cmd = await helper.Command(conn, @"
                CREATE TABLE IF NOT EXISTS wt_facility (
                    id bigint NOT NULL PRIMARY KEY,
                    zone_id int NOT NULL,
                    region_id int NOT NULL,
                    name varchar NOT NULL,
                    type_id int NOT NULL,
                    type_name varchar NOT NULL
                );
            ");

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

    }
}
