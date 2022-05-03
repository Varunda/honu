using Npgsql;
using System.Threading.Tasks;

namespace watchtower.Services.Db.Patches {

    [Patch]
    public class Patch49AlertName : IDbPatch {
        public int MinVersion => 49;
        public string Name => "add name to alerts";

        public async Task Execute(IDbHelper helper) {
            using NpgsqlConnection conn = helper.Connection();
            using NpgsqlCommand cmd = await helper.Command(conn, @"
                ALTER TABLE alerts
                    ADD COLUMN IF NOT EXISTS name varchar NOT NULL DEFAULT '';
            ");

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

    }
}
