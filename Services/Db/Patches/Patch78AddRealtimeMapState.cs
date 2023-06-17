using Npgsql;
using System.Threading.Tasks;

namespace watchtower.Services.Db.Patches {

    [Patch]
    public class Patch78AddRealtimeMapState : IDbPatch {

        public int MinVersion => 78;

        public string Name => "create realtime map state";

        public async Task Execute(IDbHelper helper) {
            using NpgsqlConnection conn = helper.Connection(Dbs.EVENTS);
            using NpgsqlCommand cmd = await helper.Command(conn, @"
                CREATE TABLE IF NOT EXISTS realtime_map_state (
                    id BIGINT NOT NULL PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
                    
                    world_id SMALLINT NOT NULL,
                    zone_id INT NOT NULL,
                    
                    timestamp timestamptz NOT NULL,
                    save_timestamp timestamptz NOT NULL,
                    
                    region_id int NOT NULL,
                    owning_faction_id int NOT NULL,
                    
                    contested bit NOT NULL,
                    contesting_faction_id INT NOT NULL,
                    
                    capture_time_ms INT NOT NULL,
                    capture_time_left_ms INT NOT NULL,
                    capture_flags_count INT NOT NULL,
                    capture_flags_left INT NOT NULL,
                    
                    faction_bounds_vs INT NOT NULL,
                    faction_bounds_nc INT NOT NULL,
                    faction_bounds_tr INT NOT NULL,
                    faction_bounds_ns INT NOT NULL,
                    
                    faction_percent_vs decimal NOT NULL,
                    faction_percent_nc decimal NOT NULL,
                    faction_percent_tr decimal NOT NULL,
                    faction_percent_ns decimal NOT NULL
                );

                CREATE INDEX IF NOT EXISTS idx_realtime_map_state_save_timestamp ON realtime_map_state(save_timestamp) INCLUDE (zone_id, world_id);
            ");

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

    }
}
