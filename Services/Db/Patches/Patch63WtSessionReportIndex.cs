using Npgsql;
using System.Threading.Tasks;

namespace watchtower.Services.Db.Patches {

    [Patch]
    public class Patch63WtSessionReportIndex : IDbPatch {
        public int MinVersion => 63;
        public string Name => "wt_session report index";

        public async Task Execute(IDbHelper helper) {
            using NpgsqlConnection conn = helper.Connection();
            using NpgsqlCommand cmd = await helper.Command(conn, @"
                CREATE INDEX IF NOT EXISTS idx_wt_session_outfit_range ON wt_session(outfit_id, start, finish);


                CREATE INDEX IF NOT EXISTS idx_wt_ledger_player_timestamp ON wt_ledger_player USING brin(timestamp);
            ");

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

    }
}
