using Npgsql;
using System.Threading.Tasks;

namespace watchtower.Services.Db.Patches {

    [Patch]
    public class Patch39AddDeletedToPsbNamed : IDbPatch {
        public int MinVersion => 39;
        public string Name => "add deleted_at to psb_named, and add psb_account_note";

        public async Task Execute(IDbHelper helper) {
            using NpgsqlConnection conn = helper.Connection();
            using NpgsqlCommand cmd = await helper.Command(conn, @"
                ALTER TABLE psb_named
                    ADD COLUMN IF NOT EXISTS created_by bigint NOT NULL DEFAULT 0; 

                ALTER TABLE psb_named
                    ADD COLUMN IF NOT EXISTS deleted_at timestamptz NULL;

                ALTER TABLE psb_named
                    ADD COLUMN IF NOT EXISTS deleted_by bigint NULL;

                CREATE TABLE IF NOT EXISTS psb_account_note (
                    id bigint NOT NULL PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
                    account_id bigint NOT NULL,
                    honu_id bigint NOT NULL,
                    timestamp timestamptz NOT NULL,
                    message varchar NOT NULL,
                    deleted_at timestamptz NULL,
                    deleted_by bigint NULL,
                    edited_at timestamptz NULL
                );

                CREATE TABLE IF NOT EXISTS honu_account (
                    id bigint NOT NULL PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
                    name varchar NOT NULL,
                    email varchar NOT NULL,
                    discord varchar NOT NULL,
                    discord_id bigint NOT NULL,
                    timestamp timestamptz NOT NULL
                );

                CREATE INDEX IF NOT EXISTS idx_psb_account_note_account_id ON psb_account_note (account_id);
            ");

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

    }
}
