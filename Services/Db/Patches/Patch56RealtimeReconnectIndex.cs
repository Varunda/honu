using Npgsql;
using System.Threading.Tasks;

namespace watchtower.Services.Db.Patches {

    [Patch]
    public class Patch56RealtimeReconnectIndex : IDbPatch {
        public int MinVersion => 56;
        public string Name => "";

        public async Task Execute(IDbHelper helper) {
            using NpgsqlConnection conn = helper.Connection();
            using NpgsqlCommand cmd = await helper.Command(conn, @"
                CREATE INDEX IF NOT EXISTS idx_realtime_reconnect_timestamp
                    ON realtime_reconnect (timestamp);

                CREATE INDEX IF NOT EXISTS idx_realtime_reconnect_world_id
                    ON realtime_reconnect (world_id);
            ");

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

    }
}
