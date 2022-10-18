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
                    item_id int NOT NULL,
                    world_id smallint NOT NULL,
                    faction_id smallint NOT NULL,
                    type_id smallint NOT NULL,
                    timestamp timestamptz NOT NULL,
                    reference_id bigint NOT NULL
                );

                CREATE INDEX IF NOT EXISTS idx_weapon_stat_top_item_id ON weapon_stat_top_xref(reference_id);
            ");

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

    }
}
