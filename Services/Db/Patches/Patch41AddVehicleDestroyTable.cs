using Npgsql;
using System.Threading.Tasks;

namespace watchtower.Services.Db.Patches {

    [Patch]
    public class Patch41AddVehicleDestroyTable : IDbPatch {

        public int MinVersion => 41;
        public string Name => "add vehicle_destroy table";

        // {
        //      "attacker_character_id":"0",
        //      "attacker_loadout_id":"0",
        //      "attacker_vehicle_id":"0",
        //      "attacker_weapon_id":"86",
        //      "character_id":"0",
        //      "event_name":"VehicleDestroy",
        //      "facility_id":"0",
        //      "faction_id":"2",
        //      "timestamp":"1642455481",
        //      "vehicle_id":"3",
        //      "world_id":"17",
        //      "zone_id":"103088492"
        //  }
        public async Task Execute(IDbHelper helper) {
            using NpgsqlConnection conn = helper.Connection();
            using NpgsqlCommand cmd = await helper.Command(conn, @"
                CREATE TABLE IF NOT EXISTS vehicle_destroy (
                    id bigint NOT NULL PRIMARY KEY GENERATED ALWAYS AS IDENTITY,

                    attacker_character_id varchar NOT NULL,
                    attacker_vehicle_id varchar NOT NULL,
                    attacker_weapon_id int NOT NULL,
                    attacker_loadout_id smallint NOT NULL,
                    attacker_team_id smallint NOT NULL,
                    attacker_faction_id smallint NOT NULL,

                    killed_character_id varchar NOT NULL,
                    killed_faction_id smallint NOT NULL,
                    killed_team_id smallint NOT NULL,
                    killed_vehicle_id varchar NOT NULL,

                    facility_id varchar NOT NULL,
                    world_id smallint NOT NULL,
                    zone_id int NOT NULL,
                    timestamp timestamptz NOT NULL
                );

                CREATE INDEX IF NOT EXISTS idx_vehicle_destroy_attacker_character_id ON vehicle_destroy(attacker_character_id);

                CREATE INDEX IF NOT EXISTS idx_vehicle_destroy_killed_character_id ON vehicle_destroy(killed_character_id);
            ");

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }
    }
}
