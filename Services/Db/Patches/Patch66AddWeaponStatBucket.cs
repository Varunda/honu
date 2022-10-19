using Npgsql;
using System.Threading.Tasks;

namespace watchtower.Services.Db.Patches {

    [Patch]
    public class Patch66AddWeaponStatBucket : IDbPatch {

        public int MinVersion => 66;
        public string Name => "create weapon_stat_bucket";

        public async Task Execute(IDbHelper helper) {
            using NpgsqlConnection conn = helper.Connection();
            using NpgsqlCommand cmd = await helper.Command(conn, @"
                CREATE TABLE IF NOT EXISTS weapon_stat_bucket (
                    id bigint NOT NULL PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
                    item_id int NOT NULL,
                    type_id smallint NOT NULL,
                    timestamp timestamptz NOT NULL,
                    start double precision NOT NULL,
                    width double precision NOT NULL,
                    count int NOT NULL
                );

                CREATE INDEX IF NOT EXISTS idx_weapon_stat_bucket_item_id ON weapon_stat_bucket(item_id);

                CREATE TABLE IF NOT EXISTS weapon_stat_top_xref (
                    id bigint NOT NULL PRIMARY KEY GENERATED ALWAYS AS IDENTITY,

                    world_id smallint NOT NULL,
                    faction_id smallint NOT NULL,
                    type_id smallint NOT NULL,
                    timestamp timestamptz NOT NULL,

                    item_id int NOT NULL,
                    character_id varchar NOT NULL,
                    vehicle_id int NOT NULL,

                    kills int NOT NULL,
                    deaths int NOT NULL,
                    headshots int NOT NULL,
                    shots int NOT NULL,
                    shots_hit int NOT NULL,
                    vehicle_kills int NOT NULL,
                    seconds_with int NOT NULL,

                    kd real NOT NULL,
                    kpm real NOT NULL,
                    vkpm real NOT NULL,
                    acc real NOT NULL,
                    hsr real NOT NULL
                );

                CREATE INDEX IF NOT EXISTS idx_weapon_stat_top_item_id ON weapon_stat_top_xref(item_id);
            ");

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

    }
}
