using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Code.Tracking;
using watchtower.Models.Db;

namespace watchtower.Services.Db {

    public class RealtimeMapStateDbStore {

        private readonly ILogger<RealtimeMapStateDbStore> _Logger;
        private readonly IDbHelper _Helper;
        private readonly IDataReader<RealtimeMapState> _Reader;

        public RealtimeMapStateDbStore(ILogger<RealtimeMapStateDbStore> logger,
            IDbHelper helper, IDataReader<RealtimeMapState> reader) {

            _Logger = logger;
            _Helper = helper;
            _Reader = reader;
        }

        /// <summary>
        ///     Insert a new <see cref="RealtimeMapState"/> into the db
        /// </summary>
        /// <param name="state">Parameters used to insert</param>
        /// <param name="cancel">Cancellation token</param>
        /// <returns>
        ///     The ID of the <see cref="RealtimeMapState"/> that was just inserted
        /// </returns>
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

        /// <summary>
        ///     Get the the <see cref="RealtimeMapState"/> for a world and region between a period
        /// </summary>
        /// <param name="worldID">ID of the world to limit the states to</param>
        /// <param name="regionID">ID of the region</param>
        /// <param name="start">When to start the period</param>
        /// <param name="end">When to end the period</param>
        /// <returns>
        ///     A list of <see cref="RealtimeMapState"/> with <see cref="RealtimeMapState.WorldID"/> of <paramref name="worldID"/>,
        ///     a <see cref="RealtimeMapState.RegionID"/> of <paramref name="regionID"/>, and a <see cref="RealtimeMapState.Timestamp"/>
        ///     between <paramref name="start"/> and <paramref name="end"/>
        /// </returns>
        public async Task<List<RealtimeMapState>> GetHistoricalByWorldAndRegion(short worldID, int regionID, DateTime start, DateTime end) {
            using Activity? root = HonuActivitySource.Root.StartActivity("realtime map state - get historical");
            root?.AddTag("honu.worldID", worldID);
            root?.AddTag("honu.regionID", regionID);
            root?.AddTag("honu.start", $"{start:u}");
            root?.AddTag("honu.end", $"{end:u}");

            using NpgsqlConnection conn = _Helper.Connection();
            using NpgsqlCommand cmd = await _Helper.Command(conn, @"
                SELECT 
                    *
                FROM 
                    realtime_map_state 
                WHERE 
                    world_id = @WorldID
                    AND region_id = @RegionID
                    AND timestamp BETWEEN @PeriodStart AND @PeriodEnd
                ORDER BY
                    save_timestamp ASC;
            ");

            cmd.AddParameter("WorldID", worldID);
            cmd.AddParameter("RegionID", regionID);
            cmd.AddParameter("PeriodStart", start);
            cmd.AddParameter("PeriodEnd", end);

            await cmd.PrepareAsync();

            List<RealtimeMapState> states = await _Reader.ReadList(cmd);
            await conn.CloseAsync();

            return states;
        }

    }
}
