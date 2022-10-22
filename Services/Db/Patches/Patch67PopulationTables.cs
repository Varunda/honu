using Npgsql;
using System.Threading.Tasks;

namespace watchtower.Services.Db.Patches {

    [Patch]
    public class Patch67PopulationTables : IDbPatch {
        public int MinVersion => 67;
        public string Name => "add population tables";

        public async Task Execute(IDbHelper helper) {
            using NpgsqlConnection conn = helper.Connection();
            using NpgsqlCommand cmd = await helper.Command(conn, @"
                CREATE INDEX IF NOT EXISTS idx_wt_session_both_range ON wt_session(finish, start) WHERE finish IS NOT NULL;

                CREATE TABLE IF NOT EXISTS population (
                    id bigint NOT NULL PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
                    timestamp timestamptz NOT NULL,
                    duration int NOT NULL,

                    world_id smallint NOT NULL,
                    faction_id smallint NOT NULL,

                    total int NOT NULL,
                    logins int NOT NULL,
                    logouts int NOT NULL,

                    unique_characters int NOT NULL,
                    seconds_played bigint NOT NULL,
                    average_session_length int NOT NULL
                );

                CREATE INDEX IF NOT EXISTS idx_population_timestamp ON population(timestamp);

                CREATE INDEX IF NOT EXISTS idx_population_world_id ON population(world_id);

                CREATE INDEX IF NOT EXISTS idx_population_faction_id ON population(faction_id);

            ");

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

    }
}
