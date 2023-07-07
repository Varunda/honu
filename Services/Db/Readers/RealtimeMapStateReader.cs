using Npgsql;
using System.Data;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Db;

namespace watchtower.Services.Db.Readers {

    public class RealtimeMapStateReader : IDataReader<RealtimeMapState> {

        public override RealtimeMapState? ReadEntry(NpgsqlDataReader reader) {
            RealtimeMapState state = new();

            state.ID = reader.GetInt64("id");
            state.WorldID = reader.GetInt16("world_id");
            state.ZoneID = reader.GetUInt32("zone_id");
            state.Timestamp = reader.GetDateTime("timestamp");
            state.SaveTimestamp = reader.GetDateTime("save_timestamp");
            state.RegionID = reader.GetInt32("region_id");
            state.OwningFactionID = reader.GetInt32("owning_faction_id");
            state.Contested = reader.GetBoolean("contested");
            state.ContestingFactionID = reader.GetInt32("contesting_faction_id");
            state.CaptureTimeMs = reader.GetInt32("capture_time_ms");
            state.CaptureTimeLeftMs = reader.GetInt32("capture_time_left_ms");
            state.CaptureFlagsCount = reader.GetInt32("capture_flags_count");
            state.CaptureFlagsLeft = reader.GetInt32("capture_flags_left");

            state.FactionBounds = new RealtimeMapStateFactionBounds();
            state.FactionBounds.VS = reader.GetInt32("faction_bounds_vs");
            state.FactionBounds.NC = reader.GetInt32("faction_bounds_nc");
            state.FactionBounds.TR = reader.GetInt32("faction_bounds_tr");
            state.FactionBounds.NS = reader.GetInt32("faction_bounds_ns");

            state.FactionPercentage = new RealtimeMapStateFactionPopulationPercentage();
            state.FactionPercentage.VS = reader.GetInt32("faction_percent_vs");
            state.FactionPercentage.NC = reader.GetInt32("faction_percent_nc");
            state.FactionPercentage.TR = reader.GetInt32("faction_percent_tr");
            state.FactionPercentage.NS = reader.GetInt32("faction_percent_ns");

            return state;
        }

    }
}
