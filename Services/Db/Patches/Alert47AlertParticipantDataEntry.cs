using Npgsql;
using System.Threading.Tasks;

namespace watchtower.Services.Db.Patches {

    public class Alert47AlertParticipantDataEntry : IDbPatch {

        public int MinVersion => 47;
        public string Name => "create alert_participant_data";

        public async Task Execute(IDbHelper helper) {
            using NpgsqlConnection conn = helper.Connection();
            using NpgsqlCommand cmd = await helper.Command(conn, @"
                CREATE TABLE IF NOT EXISTS alert_participant_data (
                    id bigint NOT NULL PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
                    alert_id bigint NOT NULL,

                    character_id varchar NOT NULL,
                    outfit_id varchar NULL,
                    seconds_online int NOT NULL,

                    kills int NOT NULL,
                    deaths int NOT NULL,
                    vehicle_kills int NOT NULL,
                    
                    heals int NOT NULL,
                    revives int NOT NULL,
                    shield_repairs int NOT NULL,
                    resupplies int NOT NULL,
                    spawns int NOT NULL,
                    repairs int NOT NULL
                );

                CREATE INDEX IF NOT EXISTS idx_alert_participant_data_alert_id ON alert_participant_data (alert_id);

                CREATE UNIQUE INDEX IF NOT EXISTS unq_alert_participant_data_character_alert ON alert_participant_data (alert_id, character_id);
            ");

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }
    }
}
