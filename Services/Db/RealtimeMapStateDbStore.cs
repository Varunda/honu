using Microsoft.Extensions.Logging;
using Npgsql;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Db;

namespace watchtower.Services.Db {

    public class RealtimeMapStateDbStore {

        private readonly ILogger<RealtimeMapStateDbStore> _Logger;
        private readonly IDbHelper _Helper;

        public RealtimeMapStateDbStore(ILogger<RealtimeMapStateDbStore> logger, IDbHelper helper) {
            _Logger = logger;
            _Helper = helper;
        }

        public async Task<long> Insert(RealtimeMapState state, CancellationToken cancel) {
            using NpgsqlConnection conn = _Helper.Connection(Dbs.EVENTS);
            using NpgsqlCommand cmd = await _Helper.Command(conn, @"
                INSERT INTO realtime_map_state (
                    zone_id, world_id,
                    timestamp, save_timestamp,
                    region_id, owning_faction_id,
                    contested, contesting_faction_id,
                    capture_time_ms, capture_time_left_ms, capture_flags_count, capture_flags_left,
                    faction_bounds_vs, faction_bounds_nc, faction_bounds_tr, faction_bounds_ns,
                    faction_percent_vs, faction_percent_nc, faction_percent_tr, faction_percent_ns
                ) VALUES (
                    @ZoneID, @WorldID,
                    @Timestamp, @SaveTimestamp,
                    @RegionID, @OwningFactionID,
                    @Contested, @ContestingFactionID,
                    @CaptureTimeMs, @CaptureTimeLeftMs, @CaptureFlagsCount, @CaptureFlagsLeft,
                    @FactionBoundsVS, @FactionBoundsNC, @FactionBoundsTR, @FactionBoundsNS,
                    @FactionPercentVS, @FactionPercentNC, @FactionPercentTR, @FactionPercentNS
                ) RETURNING id;
            ");

            cmd.AddParameter("ZoneID", state.ZoneID);
            cmd.AddParameter("WorldID", state.WorldID);
            cmd.AddParameter("Timestamp", state.Timestamp);
            cmd.AddParameter("SaveTimestamp", state.SaveTimestamp);
            cmd.AddParameter("RegionID", state.RegionID);
            cmd.AddParameter("OwningFactionID", state.OwningFactionID);
            cmd.AddParameter("Contested", state.Contested);
            cmd.AddParameter("ContestingFactionID", state.ContestingFactionID);
            cmd.AddParameter("CaptureTimeMs", state.CaptureTimeMs);
            cmd.AddParameter("CaptureTimeLeftMs", state.CaptureTimeLeftMs);
            cmd.AddParameter("CaptureFlagsCount", state.CaptureFlagsCount);
            cmd.AddParameter("CaptureFlagsLeft", state.CaptureFlagsLeft);
            cmd.AddParameter("FactionBoundsVS", state.FactionBounds.VS);
            cmd.AddParameter("FactionBoundsNC", state.FactionBounds.NC);
            cmd.AddParameter("FactionBoundsTR", state.FactionBounds.TR);
            cmd.AddParameter("FactionBoundsNS", state.FactionBounds.NS);
            cmd.AddParameter("FactionPercentVS", state.FactionPercentage.VS);
            cmd.AddParameter("FactionPercentNC", state.FactionPercentage.NC);
            cmd.AddParameter("FactionPercentTR", state.FactionPercentage.TR);
            cmd.AddParameter("FactionPercentNS", state.FactionPercentage.NS);

            await cmd.PrepareAsync();

            long ID = await cmd.ExecuteInt64(cancel);

            return ID;
        }

    }
}
