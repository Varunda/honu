using Npgsql;
using System.Threading.Tasks;

namespace watchtower.Services.Db.Patches {

    [Patch]
    public class Patch89AddWorldChanges : IDbPatch {
        public int MinVersion => 89;
        public string Name => "create world_changes table";

        public async Task Execute(IDbHelper helper) {
            using NpgsqlConnection conn = helper.Connection(Dbs.EVENTS);
            using NpgsqlCommand cmd = await helper.Command(conn, @"
                CREATE TABLE IF NOT EXISTS world_changes (
                    character_id varchar NOT NULL,
                    world_id smallint NOT NULL,
                    timestamp timestamptz NOT NULL
                );

                CREATE INDEX IF NOT EXISTS idx_world_changes_character_id ON world_changes(character_id);
            ");

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

    }
}
