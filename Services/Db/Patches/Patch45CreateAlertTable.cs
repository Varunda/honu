using Npgsql;
using System.Threading.Tasks;

namespace watchtower.Services.Db.Patches {

    [Patch]
    public class Patch45CreateAlertTable : IDbPatch {
        public int MinVersion => 45;
        public string Name => "create alerts table";

        public async Task Execute(IDbHelper helper) {
            using NpgsqlConnection conn = helper.Connection();
            using NpgsqlCommand cmd = await helper.Command(conn, @"
                CREATE TABLE IF NOT EXISTS alerts (
                    id bigint NOT NULL PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
                    timestamp timestamptz NOT NULL,
                    duration int NOT NULL,
                    zone_id int NOT NULL,
                    world_id smallint NOT NULL,
                    alert_id int NOT NULL,
                    victor_faction_id smallint NULL,
                    warpgate_vs int NOT NULL,
                    warpgate_nc int NOT NULL,
                    warpgate_tr int NOT NULL,
                    zone_facility_count int NOT NULL,
                    count_vs int NULL,
                    count_nc int NULL,
                    count_tr int NULL
                );
            ");

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }
    }
}
