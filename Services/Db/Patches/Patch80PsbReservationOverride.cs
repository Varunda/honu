using Npgsql;
using System.Threading.Tasks;

namespace watchtower.Services.Db.Patches {

    public class Patch80PsbReservationOverride : IDbPatch {
        public int MinVersion => 80;
        public string Name => "psb reservation overrides";

        public async Task Execute(IDbHelper helper) {
            using NpgsqlConnection conn = helper.Connection();
            using NpgsqlCommand cmd = await helper.Command(conn, @"
                ALTER TABLE psb_parsed_reservations
                    ADD COLUMN IF NOT EXISTS override_by bigint NULL;
            ");

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

    }
}
