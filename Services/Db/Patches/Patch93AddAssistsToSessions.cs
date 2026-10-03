using Npgsql;
using System.Threading.Tasks;

namespace watchtower.Services.Db.Patches {

    [Patch]
    public class Patch93AddAssistsToSessions : IDbPatch {
        public int MinVersion => 93;
        public string Name => "add assists to wt_session";

        public async Task Execute(IDbHelper helper) {
            using NpgsqlConnection conn = helper.Connection(Dbs.CHARACTER);
            using NpgsqlCommand cmd = await helper.Command(conn, @"
                ALTER TABLE wt_session
                    ADD COLUMN IF NOT EXISTS assists int NOT NULL DEFAULT -1;
            ");

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

    }
}
