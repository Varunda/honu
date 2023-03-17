using Npgsql;
using System.Threading.Tasks;

namespace watchtower.Services.Db.Patches {

    [Patch]
    public class Patch73AddParsedMetadata : IDbPatch {
        public int MinVersion => 73;
        public string Name => "create psb_parsed_reservation";

        public async Task Execute(IDbHelper helper) {
            using NpgsqlConnection conn = helper.Connection();
            using NpgsqlCommand cmd = await helper.Command(conn, @"
                CREATE TABLE IF NOT EXISTS psb_parsed_reservations (
                    message_id bigint PRIMARY KEY NOT NULL,
                    account_sheet_id varchar NULL,
                    account_sheet_approved_by bigint NULL,
                    booking_approved_by bigint NULL
                );
            ");

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

    }
}
