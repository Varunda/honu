using Npgsql;
using System.Threading.Tasks;

namespace watchtower.Services.Db.Patches {

    [Patch]
    public class Patch59WtSessionIndexes : IDbPatch {
        public int MinVersion => 59;
        public string Name => "create wt_session indexes";

        public async Task Execute(IDbHelper helper) {
            using NpgsqlConnection conn = helper.Connection();
            await using NpgsqlCommand cmd = await helper.Command(conn, @"
                CREATE INDEX IF NOT EXISTS idx_wt_session_outfit_id ON wt_session(outfit_id);
            ");

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

    }
}
