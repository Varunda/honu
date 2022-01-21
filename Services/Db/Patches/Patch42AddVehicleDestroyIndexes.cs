using Npgsql;
using System.Threading.Tasks;

namespace watchtower.Services.Db.Patches {

    [Patch]
    public class Patch42AddVehicleDestroyIndexes : IDbPatch {
        public int MinVersion => 42;
        public string Name => "add indexes to vehicle_destroy";

        public async Task Execute(IDbHelper helper) {
            using NpgsqlConnection conn = helper.Connection();
            using NpgsqlCommand cmd = await helper.Command(conn, @"
                CREATE INDEX IF NOT EXISTS idx_vehicle_destroy_timestamp ON vehicle_destroy USING brin(timestamp);

                CREATE INDEX IF NOT EXISTS idx_vehicle_destroy_attacker_character_id ON vehicle_destroy (attacker_character_id);

                CREATE INDEX IF NOT EXISTS idx_vehicle_destroy_killed_character_id ON vehicle_destroy (killed_character_id);

                CREATE TABLE IF NOT EXISTS vehicle (
                    id int NOT NULL PRIMARY KEY,
                    name varchar NOT NULL,
                    description varchar NOT NULL,
                    type_id int NOT NULL,
                    cost_resource_id int NOT NULL,
                    image_set_id int NOT NULL,
                    image_id int NOT NULL
                );
            ");

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

    }
}
