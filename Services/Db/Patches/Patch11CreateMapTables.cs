using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Services.Db.Patches {

    [Patch]
    public class Patch11CreateMapTables : IDbPatch {

        public int MinVersion => 11;
        public string Name => "add ledger_map_hex and ledger_map_link";

        public async Task Execute(IDbHelper helper) {
            using NpgsqlConnection conn = helper.Connection();
            using NpgsqlCommand cmd = await helper.Command(conn, @"
                CREATE TABLE IF NOT EXISTS ledger_map_hex (
                    region_id int NOT NULL,
                    zone_id int NOT NULL,
                    location_x int NOT NULL,
                    location_y int NOT NULL,
                    type_id int NOT NULL,

                    PRIMARY KEY (zone_id, location_x, location_y)
                );

                CREATE TABLE IF NOT EXISTS ledger_map_link (
                    facility_a int NOT NULL,
                    facility_b int NOT NULL,
                    zone_id int NOT NULL,
                    description varchar NULL,

                    PRIMARY KEY (facility_a, facility_b)
                );

                ALTER TABLE wt_facility
                    ADD IF NOT EXISTS location_x real NULL;
                ALTER TABLE wt_facility
                    ADD IF NOT EXISTS location_y real NULL;
                ALTER TABLE wt_facility
                    ADD IF NOT EXISTS location_z real NULL;
            ");

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

    }
}
