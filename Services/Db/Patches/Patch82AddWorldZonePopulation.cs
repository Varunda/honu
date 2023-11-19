using Npgsql;
using System.Threading.Tasks;

namespace watchtower.Services.Db.Patches {

    [Patch]
    public class Patch82AddWorldZonePopulation : IDbPatch {
        public int MinVersion => 82;

        public string Name => "add world_zone_population";

        public async Task Execute(IDbHelper helper) {

            using NpgsqlConnection conn = helper.Connection(Dbs.EVENTS);
            using NpgsqlCommand cmd = await helper.Command(conn, @"
                CREATE TABLE IF NOT EXISTS world_zone_population (
                    world_id smallint NOT NULL,
                    zone_id int NOT NULL,
                    timestamp timestamptz NOT NULL,

                    total int NOT NULL,
                    faction_vs int NOT NULL,
                    faction_nc int NOT NULL,
                    faction_tr int NOT NULL,
                    faction_ns int NOT NULL,

                    team_vs int NOT NULL,
                    team_nc int NOT NULL,
                    team_tr int NOT NULL,
                    team_unknown int NOT NULL,

                    PRIMARY KEY (world_id, zone_id, timestamp)
                );
            ");

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

    }
}
