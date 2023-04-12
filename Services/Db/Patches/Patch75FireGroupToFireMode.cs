using Npgsql;
using System.Threading.Tasks;

namespace watchtower.Services.Db.Patches {

    [Patch]
    public class Patch75FireGroupToFireMode : IDbPatch {
        public int MinVersion => 75;
        public string Name => "create fire_group_to_fire_mode";

        public async Task Execute(IDbHelper helper) {
            using NpgsqlConnection conn = helper.Connection();
            using NpgsqlCommand cmd = await helper.Command(conn, @"
                CREATE TABLE IF NOT EXISTS fire_group_to_fire_mode (
                    fire_group_id int NOT NULL,
                    fire_mode_id int NOT NULL,
                    fire_mode_index int NOT NULL,
                    PRIMARY KEY (fire_group_id, fire_mode_id, fire_mode_index)
                );
            ");

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

    }
}
