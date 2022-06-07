using Npgsql;
using System.Threading.Tasks;

namespace watchtower.Services.Db.Patches {

    [Patch]
    public class Patch54RealtimeReconnectTable : IDbPatch {
        public int MinVersion => 54;

        public string Name => "create realtime_reconnect tabel";

        public async Task Execute(IDbHelper helper) {
            using NpgsqlConnection conn = helper.Connection();
            using NpgsqlCommand cmd = await helper.Command(conn, @"
                CREATE TABLE IF NOT EXISTS realtime_reconnect (
                    ID bigint NOT NULL GENERATED ALWAYS AS IDENTITY,
                    timestamp timestamptz NOT NULL,
                    world_id smallint NOT NULL,
                    stream_type varchar NOT NULL,
                    failed_count int NOT NULL,
                    duration int NOT NULL,
                    event_count int NOT NULL
                );
            ");

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

    }
}
