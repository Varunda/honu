using System.Text.Json;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Db;

namespace watchtower.Services.Census.Readers {

    public class CensusRealtimeMapStateReader : ICensusReader<RealtimeMapState> {

        public override RealtimeMapState? ReadEntry(JsonElement token) {
            RealtimeMapState state = new();

            state.WorldID = token.GetWorldID();

            ushort definitionID = token.GetUInt16("zone_id");
            ushort instanceID = token.GetUInt16("zone_instance");

            state.ZoneID = (uint)((instanceID << 16) | definitionID);
            state.Timestamp = token.CensusTimestamp("timestamp");
            state.RegionID = token.GetRequiredInt32("map_region_id");
            state.OwningFactionID = token.GetInt32("owning_faction_id", 0);
            state.Contested = token.GetBoolean("contested", false);
            state.ContestingFactionID = token.GetInt32("contesting_faction_id", 0);
            state.CaptureTimeMs = token.GetInt32("capture_time_ms", -1);
            state.CaptureTimeLeftMs = token.GetInt32("remaining_capture_time_ms", -1);
            state.CaptureFlagsCount = token.GetInt32("ctf_flags", 0);
            state.CaptureFlagsLeft = token.GetInt32("remaining_ctf_flags", 0);

            JsonElement? bound = token.GetChild("faction_population_upper_bound");
            if (bound != null) {
                state.FactionBounds = new RealtimeMapStateFactionBounds();
                state.FactionBounds.VS = bound.Value.GetRequiredInt32("VS");
                state.FactionBounds.NC = bound.Value.GetRequiredInt32("NC");
                state.FactionBounds.TR = bound.Value.GetRequiredInt32("TR");
                state.FactionBounds.NS = bound.Value.GetRequiredInt32("NSO");
            }

            JsonElement? percent = token.GetChild("faction_population_percentage");
            if (percent != null) {
                state.FactionPercentage = new RealtimeMapStateFactionPopulationPercentage();
                state.FactionPercentage.VS = percent.Value.GetDecimal("VS", 0m);
                state.FactionPercentage.NC = percent.Value.GetDecimal("NC", 0m);
                state.FactionPercentage.TR = percent.Value.GetDecimal("TR", 0m);
                state.FactionPercentage.NS = percent.Value.GetDecimal("NSO", 0m);
            }

            return state;
        }

    }
}
