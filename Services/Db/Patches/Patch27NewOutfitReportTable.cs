using Npgsql;
using System.Threading.Tasks;

namespace watchtower.Services.Db.Patches {

    [Patch]
    public class Patch27NewOutfitReportTable : IDbPatch {
        public int MinVersion => 27;

        public string Name => "create new outfit_report table, dropping the old one";

        public async Task Execute(IDbHelper helper) {
            using NpgsqlConnection conn = helper.Connection();
            using NpgsqlCommand cmd = await helper.Command(conn, @"
                DROP TABLE IF EXISTS outfit_report;
                
                CREATE TABLE IF NOT EXISTS outfit_report (
                    id uuid NOT NULL PRIMARY KEY,
                    generator varchar NOT NULL,
                    timestamp timestamptz NOT NULL,
                    period_start timestamptz NOT NULL,
                    period_end timestamptz NOT NULL
                );
            ");

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

    }
}
