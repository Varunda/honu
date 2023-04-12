using Npgsql;
using System.Threading.Tasks;

namespace watchtower.Services.Db.Patches {

    [Patch]
    public class Patch74WrappedEntries : IDbPatch {
        public int MinVersion => 74;
        public string Name => "create wrapped_entries table";

        public async Task Execute(IDbHelper helper) {
            using NpgsqlConnection conn = helper.Connection();
            using NpgsqlCommand cmd = await helper.Command(conn, @"
                CREATE TABLE IF NOT EXISTS wrapped_entries (
                    id uuid NOT NULL PRIMARY KEY,
                    input_character_ids varchar NOT NULL,
                    timestamp timestamptz NOT NULL
                );
            ");

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

    }
}
