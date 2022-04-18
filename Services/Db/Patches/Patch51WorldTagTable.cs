using Npgsql;
using System.Threading.Tasks;

namespace watchtower.Services.Db.Patches {

    [Patch]
    public class Patch51WorldTagTable : IDbPatch {

        public int MinVersion => 51;
        public string Name => "Create the world_tag table";

        public async Task Execute(IDbHelper helper) {
            using NpgsqlConnection conn = helper.Connection();
            using NpgsqlCommand cmd = await helper.Command(conn, @"
                CREATE TABLE IF NOT EXISTS world_tag (
                    id bigint NOT NULL PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
                    character_id varchar NOT NULL,
                    world_id smallint NOT NULL,
                    timestamp timestamptz NOT NULL,
                    target_killed timestamptz NULL,
                    kills int NOT NULL,
                    was_killed bool NOT NULL
                );
            ");

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }
    }
}
