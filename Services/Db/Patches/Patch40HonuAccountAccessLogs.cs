using Npgsql;
using System.Threading.Tasks;

namespace watchtower.Services.Db.Patches {

    [Patch]
    public class Patch40HonuAccountAccessLogs : IDbPatch {
        public int MinVersion => 40;
        public string Name => "Add honu_account_access_logs";

        public async Task Execute(IDbHelper helper) {
            using NpgsqlConnection conn = helper.Connection();
            using NpgsqlCommand cmd = await helper.Command(conn, @"
                CREATE TABLE IF NOT EXISTS honu_account_access_logs (
                    id bigint NOT NULL GENERATED ALWAYS AS IDENTITY,
                    timestamp timestamptz NOT NULL,
                    success boolean NOT NULL,
                    honu_id bigint NULL,
                    email varchar NULL
                );
            ");

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

    }
}
