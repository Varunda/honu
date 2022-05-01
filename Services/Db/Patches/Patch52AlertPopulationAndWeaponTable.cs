using Npgsql;
using System.Threading.Tasks;

namespace watchtower.Services.Db.Patches {

    [Patch]
    public class Path52AlertPopulationAndWeaponTable : IDbPatch {
        public int MinVersion => 52;
        public string Name => "create alert_population and alert_weapon";

        public async Task Execute(IDbHelper helper) {
            using NpgsqlConnection conn = helper.Connection();
            using NpgsqlCommand cmd = await helper.Command(conn, @"
                CREATE TABLE IF NOT EXISTS alert_population (
                    id bigint NOT NULL PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
                    alert_id bigint NOT NULL,
                    timestamp timestamptz NOT NULL,
                    count_vs int NOT NULL,
                    count_nc int NOT NULL,
                    count_tr int NOT NULL,
                    count_unknown int NOT NULL
                );

                CREATE TABLE IF NOT EXISTS alert_weapons (
                    id bigint NOT NULL PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
                    alert_id bigint NOT NULL,
                    item_id int NOT NULL,
                    faction_id smallint NOT NULL,
                    kills int NOT NULL,
                    headshot_kills int NOT NULL,
                    unique_users int NOT NULL
                );
            ");

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

    }
}
