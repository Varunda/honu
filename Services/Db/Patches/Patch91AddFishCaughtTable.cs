using Npgsql;
using System.Threading.Tasks;

namespace watchtower.Services.Db.Patches {

    [Patch]
    public class Patch91AddFishCaughtTable : IDbPatch {
        public int MinVersion => 91;

        public string Name => "add fish_caught table";

        public async Task Execute(IDbHelper helper) {
            using NpgsqlConnection conn = helper.Connection(Dbs.EVENTS);
            using NpgsqlCommand cmd = await helper.Command(conn, @"
                CREATE TABLE IF NOT EXISTS fish_caught (
                    id bigint NOT NULL PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
                    world_id smallint NOT NULL,
                    zone_id int NOT NULL,
                    timestamp timestamptz NOT NULL,
                    fish_id int NOT NULL,
                    character_id varchar NOT NULL,
                    team_id smallint NOT NULL,
                    loadout_id smallint NOT NULL
                );
            ");

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

    }
}
