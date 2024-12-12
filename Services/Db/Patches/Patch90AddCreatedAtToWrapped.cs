using Npgsql;
using System.Threading.Tasks;

namespace watchtower.Services.Db.Patches {

    [Patch]
    public class Patch90AddCreatedAtToWrapped : IDbPatch {
        public int MinVersion => 90;
        public string Name => "add created_at to wrapped";

        public async Task Execute(IDbHelper helper) {
            using NpgsqlConnection conn = helper.Connection(Dbs.EVENTS);
            using NpgsqlCommand cmd = await helper.Command(conn, @"
                ALTER TABLE wrapped_entries 
                    ADD COLUMN IF NOT EXISTS created_at timestamptz NULL;

                update wrapped_entries
                    SET created_at = timestamp
                    WHERE created_at IS NULL;

                ALTER TABLE wrapped_entries
                    ALTER COLUMN created_at SET NOT NULL;
            ");

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

    }
}
