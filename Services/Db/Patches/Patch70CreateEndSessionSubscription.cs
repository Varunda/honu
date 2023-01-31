using Npgsql;
using System.Threading.Tasks;

namespace watchtower.Services.Db.Patches {

    [Patch]
    public class Patch70CreateEndSessionSubscription : IDbPatch {
        public int MinVersion => 70;
        public string Name => "create session_end_subscription";

        public async Task Execute(IDbHelper helper) {

            using NpgsqlConnection conn = helper.Connection();
            using NpgsqlCommand cmd = await helper.Command(conn, @"
                CREATE TABLE IF NOT EXISTS session_end_subscription (
                    id BIGINT NOT NULL PRIMARY KEY GENERATED ALWAYS AS IDENTITY,

                    discord_id bigint NOT NULL,
                    character_id varchar NOT NULL,
                    timestamp timestamptz NOT NULL
                );

                CREATE INDEX IF NOT EXISTS idx_session_end_subscription_discord_id ON session_end_subscription(discord_id);

                CREATE INDEX IF NOT EXISTS idx_session_end_subscription_character_id ON session_end_subscription(character_id);
            ");

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

    }
}
