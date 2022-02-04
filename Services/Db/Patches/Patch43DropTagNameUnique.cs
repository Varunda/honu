using Npgsql;
using System.Threading.Tasks;

namespace watchtower.Services.Db.Patches {

    public class Patch43DropTagNameUnique : IDbPatch {
        public int MinVersion => 43;
        public string Name => "Drop psb_named_tag_name_key";

        public async Task Execute(IDbHelper helper) {
            using NpgsqlConnection conn = helper.Connection();
            using NpgsqlCommand cmd = await helper.Command(conn, @"
                ALTER TABLE psb_named
                    DROP CONSTRAINT IF EXISTS psb_named_tag_name_key;
            ");

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

    }
}
