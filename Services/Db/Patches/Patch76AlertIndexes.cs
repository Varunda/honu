using Npgsql;
using System.Threading.Tasks;

namespace watchtower.Services.Db.Patches {

    [Patch]
    public class Patch76AlertIndexes : IDbPatch {
        public int MinVersion => 76;

        public string Name => "create indexes for getting alerts";

        public async Task Execute(IDbHelper helper) {
            using NpgsqlConnection conn = helper.Connection();
            using NpgsqlCommand cmd = await helper.Command(conn, @"
                CREATE INDEX IF NOT EXISTS idx_alert_participant_data_character_id_alert_id
                    ON alert_participant_data (character_id, alert_id);

                CREATE INDEX IF NOT EXISTS idx_alerts_timestamp
                    ON alerts (timestamp);

                ALTER TABLE alert_participant_data
                    ADD COLUMN IF NOT EXISTS timestamp timestamptz NULL;
                    
                UPDATE alert_participant_data apd
                    SET timestamp = sq.timestamp
                    FROM (
                        SELECT id, timestamp
                            FROM alerts
                     ) AS sq
                     WHERE apd.alert_id = sq.id AND apd.timestamp IS NULL;

                ALTER TABLE alert_participant_data
                    ALTER COLUMN timestamp SET NOT NULL;

                CREATE INDEX IF NOT EXISTS idx_alert_participant_character_id_timestamp
                    ON alert_participant_data (character_id, timestamp);
            ");

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

    }
}
