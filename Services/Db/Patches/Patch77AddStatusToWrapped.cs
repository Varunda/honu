using Npgsql;
using System.Threading.Tasks;

namespace watchtower.Services.Db.Patches {

    [Patch]
    public class Patch77AddStatusToWrapped : IDbPatch {
        public int MinVersion => 77;
        public string Name => "add status to wrapped";

        public async Task Execute(IDbHelper helper) {
            using NpgsqlConnection conn = helper.Connection();
            using NpgsqlCommand cmd = await helper.Command(conn, @"
                ALTER TABLE wrapped_entries
                    ADD COLUMN IF NOT EXISTS status int NOT NULL;

                CREATE TABLE IF NOT EXISTS wrapped_entry_status (
                    id int NOT NULL PRIMARY KEY,
                    name varchar NOT NULL
                );

                INSERT INTO wrapped_entry_status
                    (id, name)
                VALUES
                    (1, 'not_started'),
                    (2, 'in_progress'),
                    (3, 'done')
                ON CONFLICT (id) DO NOTHING;
            ");

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

    }

}
