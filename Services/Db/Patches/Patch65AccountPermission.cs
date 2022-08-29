using Npgsql;
using System.Threading.Tasks;

namespace watchtower.Services.Db.Patches {

    [Patch]
    public class Patch65AccountPermission : IDbPatch {
        public int MinVersion => 65;
        public string Name => "add account_permission";

        public async Task Execute(IDbHelper helper) {
            using NpgsqlConnection conn = helper.Connection();
            using NpgsqlCommand cmd = await helper.Command(conn, @"
                CREATE TABLE IF NOT EXISTS honu_account_permission (
                    id bigint NOT NULL PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
                    account_id bigint NOT NULL,
                    permission varchar NOT NULL,
                    timestamp timestamptz NOT NULL,
                    granted_by_id bigint NOT NULL
                );
            
                CREATE INDEX IF NOT EXISTS idx_honu_account_permission_account_id ON honu_account_permission(account_id);

                CREATE INDEX IF NOT EXISTS idx_honu_account_permission_permisison ON honu_account_permission(permission);

                CREATE INDEX IF NOT EXISTS idx_honu_account_permssion_granted_by_id ON honu_account_permission(granted_by_id);

                CREATE TABLE IF NOT EXISTS honu_permission (
                    id VARCHAR NOT NULL PRIMARY KEY,
                    description VARCHAR NOT NULL
                );

                ALTER TABLE honu_account
                    ADD COLUMN IF NOT EXISTS deleted_on timestamptz NULL;

                ALTER TABLE honu_account
                    ADD COLUMN IF NOT EXISTS deleted_by bigint NULL;
            ");

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

    }
}
