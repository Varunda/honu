using Npgsql;
using System.Threading.Tasks;

namespace watchtower.Services.Db.Patches {

    [Patch]
    public class Patch88AddVehicleUsageData : IDbPatch {
        public int MinVersion => 88;
        public string Name => "add vehicle_usage";

        public async Task Execute(IDbHelper helper) {
            using NpgsqlConnection conn = helper.Connection(Dbs.EVENTS);
            using NpgsqlCommand cmd = await helper.Command(conn, @"
                create table if not exists vehicle_usage (
                    id bigint NOT NULL PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
                    world_id smallint NOT NULL,
                    zone_id int NOT NULL,
                    timestamp timestamptz NOT NULL,
                    total_players int NOT NULL,
                    usage_vs jsonb NOT NULL,
                    usage_nc jsonb NOT NULL,
                    usage_tr jsonb NOT NULL,
                    usage_other jsonb NOT NULL
                );
            ");

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

    }
}
