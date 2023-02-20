using Npgsql;
using System.Threading.Tasks;

namespace watchtower.Services.Db.Patches {

    [Patch]
    public class Patch71CreateAlertEndSubscription : IDbPatch {
        public int MinVersion => 71;

        public string Name => "create alert_end_subscription";

        public async Task Execute(IDbHelper helper) {
            using NpgsqlConnection conn = helper.Connection();
            using NpgsqlCommand cmd = await helper.Command(conn, @"
                CREATE TABLE IF NOT EXISTS alert_end_subscription (
                    id BIGINT NOT NULL PRIMARY KEY GENERATED ALWAYS AS IDENTITY,

                    created_by_id bigint NOT NULL,
                    timestamp timestamptz NOT NULL,

                    guild_id bigint NULL,
                    channel_id bigint NULL,
                    
                    world_id smallint NULL,
                    outfit_id varchar NULL,
                    character_id varchar NULL,

                    world_character_minimum bigint NOT NULL,
                    outfit_character_minimum bigint NOT NULL,
                    character_minimum_seconds bigint NOT NULL
                );

                CREATE INDEX IF NOT EXISTS idx_alert_end_subscription_created_by_id ON alert_end_subscription(created_by_id);

                CREATE INDEX IF NOT EXISTS idx_alert_end_subscription_channel_id ON alert_end_subscription(channel_id);
            ");

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }
    }
}
