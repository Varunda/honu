using Npgsql;
using System.Threading.Tasks;

namespace watchtower.Services.Db.Patches {
    [Patch]
    public class Patch55DataminedDirective : IDbPatch {
        public int MinVersion => 55;

        public string Name => "update directive tables for the datamined data";

        public async Task Execute(IDbHelper helper) {
            using NpgsqlConnection conn = helper.Connection();
            using NpgsqlCommand cmd = await helper.Command(conn, @"
                ALTER TABLE directive
                    ALTER COLUMN objective_set_id DROP NOT NULL;
            ");

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

    }
}
