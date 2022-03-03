using Npgsql;
using System.Threading.Tasks;

namespace watchtower.Services.Db.Patches {

    [Patch]
    public class Patch46AddVehicleStats : IDbPatch {
        public int MinVersion => 46;

        public string Name => "add vehicle_id to weapon_stats";

        public async Task Execute(IDbHelper helper) {
            using NpgsqlConnection conn = helper.Connection();
            using NpgsqlCommand cmd = await helper.Command(conn, @"
                ALTER TABLE weapon_stats
                    ADD COLUMN IF NOT EXISTS vehicle_id int NOT NULL DEFAULT 0;
            ");

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

    }
}
