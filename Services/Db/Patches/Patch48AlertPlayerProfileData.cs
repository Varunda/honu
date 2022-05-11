using Npgsql;
using System.Threading.Tasks;

namespace watchtower.Services.Db.Patches {

    [Patch]
    public class Patch48AlertPlayerProfileData : IDbPatch {
        public int MinVersion => 48;
        public string Name => "create alert_player_profile_data";

        public async Task Execute(IDbHelper helper) {
            using NpgsqlConnection conn = helper.Connection();
            using NpgsqlCommand cmd = await helper.Command(conn, @"
                CREATE TABLE alert_player_profile_data (
                    id bigint NOT NULL PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
                    alert_id bigint NOT NULL,
                    character_id varchar NOT NULL,
                    profile_id smallint NOT NULL,
                    kills int NOT NULL,
                    deaths int NOT NULL,
                    vehicle_kills int NOT NULL,
                    time_as int NOT NULL
                );

                CREATE INDEX IF NOT EXISTS idx_alert_player_profile_data_alert_id ON alert_player_profile_data (alert_id);
            ");

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

    }
}
