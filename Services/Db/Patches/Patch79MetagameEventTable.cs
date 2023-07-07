using Npgsql;
using System.Threading.Tasks;

namespace watchtower.Services.Db.Patches {

    [Patch]
    public class Patch79MetagameEventTable : IDbPatch {
        public int MinVersion => 79;

        public string Name => "create metagame_event";

        public async Task Execute(IDbHelper helper) {
            NpgsqlConnection conn = helper.Connection();
            NpgsqlCommand cmd = await helper.Command(conn, @"
                CREATE TABLE IF NOT EXISTS metagame_event (
                    id INT PRIMARY KEY NOT NULL,
                    name varchar NOT NULL,
                    description varchar NOT NULL,
                    type_id int NOT NULL
                );
            ");

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

    }
}
