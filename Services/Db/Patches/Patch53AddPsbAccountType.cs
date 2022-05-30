using Npgsql;
using System.Threading.Tasks;

namespace watchtower.Services.Db.Patches {

    [Patch]
    public class Patch53AddPsbAccountType : IDbPatch {
        public int MinVersion => 53;
        public string Name => "rename psb_named to psb_account, add account_type";

        public async Task Execute(IDbHelper helper) {
            using NpgsqlConnection conn = helper.Connection();
            using NpgsqlCommand cmd = await helper.Command(conn, @"
                ALTER TABLE psb_named
                    ADD COLUMN IF NOT EXISTS account_type smallint NOT NULL DEFAULT 0;
                
                CREATE TABLE IF NOT EXISTS psb_account_type (
                    id smallint NOT NULL,
                    name varchar NOT NULL
                );

                INSERT INTO psb_account_type (id, name) VALUES
                    (1, 'named'),
                    (2, 'practice'),
                    (3, 'ovo');
            ");

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

    }
}
