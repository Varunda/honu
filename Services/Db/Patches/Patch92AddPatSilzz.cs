using Npgsql;
using System.Threading.Tasks;

namespace watchtower.Services.Db.Patches {

    [Patch]
    public class Patch92AddPatSilzz : IDbPatch {
        public int MinVersion => 92;
        public string Name => "add pat_silzz";

        public async Task Execute(IDbHelper helper) {
            using NpgsqlConnection conn = helper.Connection(Dbs.EVENTS);
            using NpgsqlCommand cmd = await helper.Command(conn, @"
                CREATE TABLE IF NOT EXISTS pat_silzz (
                    key varchar NOT NULL PRIMARY KEY,
                    value bigint NOT NULL,
                    timestamp timestamptz NOT NULL
                );

                INSERT INTO pat_silzz (
                    key, value, timestamp
                ) VALUES (
                    'silzz', 0, NOW() at time zone 'utc'
                ) ON CONFLICT (key) DO NOTHING;
            ");

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

    }
}
