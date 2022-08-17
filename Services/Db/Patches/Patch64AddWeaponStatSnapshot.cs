using Npgsql;
using System.Threading.Tasks;

namespace watchtower.Services.Db.Patches {
    [Patch]
    public class Patch64AddWeaponStatSnapshot : IDbPatch {
        public int MinVersion => 64;

        public string Name => "add weapon_stat_snapshot";

        public async Task Execute(IDbHelper helper) {
            using NpgsqlConnection conn = helper.Connection();
            using NpgsqlCommand cmd = await helper.Command(conn, @"
                CREATE TABLE IF NOT EXISTS weapon_stat_snapshot (
                    id bigint NOT NULL PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
                    item_id int NOT NULL,
                    timestamp timestamptz NOT NULL,
                    users int NOT NULL,
                    kills bigint NOT NULL,
                    deaths bigint NOT NULL,
                    shots bigint NOT NULL,
                    shots_hit bigint NOT NULL,
                    headshots bigint NOT NULL,
                    vehicle_kills bigint NOT NULL,
                    seconds_with bigint NOT NULL
                );

                CREATE INDEX IF NOT EXISTS idx_weapon_stat_snapshot_item_id ON weapon_stat_snapshot(item_id);
            ");

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

    }
}
