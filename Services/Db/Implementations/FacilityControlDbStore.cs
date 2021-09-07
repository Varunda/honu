using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Code.Constants;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Db;
using watchtower.Models.Events;

namespace watchtower.Services.Db.Implementations {

    public class FacilityControlDbStore : IFacilityControlDbStore {

        private readonly ILogger<FacilityControlDbStore> _Logger;
        private readonly IDbHelper _DbHelper;
        private readonly IDataReader<FacilityControlDbEntry> _ControlReader;

        public FacilityControlDbStore(ILogger<FacilityControlDbStore> logger,
            IDbHelper helper, IDataReader<FacilityControlDbEntry> reader) {

            _Logger = logger;
            _DbHelper = helper;
            _ControlReader = reader ?? throw new ArgumentNullException(nameof(reader));
        }

        public async Task<List<FacilityControlDbEntry>> Get(FacilityControlOptions parameters) {
            string periodStartWhere = parameters.PeriodStart == null ? "" : "AND timestamp <= @PeriodStart ";
            string periodEndWhere = parameters.PeriodEnd == null ? "" : "AND timestamp >= @PeriodEnd ";
            string zoneIDWhere = parameters.ZoneID == null ? "" : "AND zone_id = @ZoneID ";
            string stateWhere = parameters.UnstableState == null ? "" : "AND zone_state = @ZoneState ";

            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @$"
                SELECT
                    facility_id,
                    COUNT(facility_id) FILTER (WHERE old_faction_id != new_faction_id) AS captured,
                    COUNT(facility_id) FILTER (WHERE old_faction_id = new_faction_id) AS defended,
                    COALESCE(AVG(players) FILTER(WHERE old_faction_id != new_faction_id), 0) AS capture_average,
                    COALESCE(AVG(players) FILTER(WHERE old_faction_id = new_faction_id), 0) AS defend_average,
                    AVG(players) AS total_average
                FROM 
                    wt_ledger
                WHERE
                    players >= @PlayerThreshold
                    AND world_id = ANY(@Worlds)
                    {periodStartWhere}
                    {periodEndWhere}
                    {zoneIDWhere}
                GROUP BY facility_id;
            ");

            List<short> worldIDs = parameters.WorldIDs.Count == 0 ? new() { World.Connery, World.Cobalt, World.Emerald, World.Miller, World.SolTech } : parameters.WorldIDs;

            cmd.AddParameter("PlayerThreshold", parameters.PlayerThreshold);
            cmd.AddParameter("ZoneID", parameters.ZoneID);
            cmd.AddParameter("PeriodStart", parameters.PeriodStart);
            cmd.AddParameter("PeriodEnd", parameters.PeriodEnd);
            cmd.AddParameter("Worlds", worldIDs);
            cmd.AddParameter("ZoneState", (int?)parameters.UnstableState);

            List<FacilityControlDbEntry> entries = await _ControlReader.ReadList(cmd);
            await conn.CloseAsync();

            return entries;
        }

        public async Task Insert(FacilityControlEvent ev) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                INSERT INTO wt_ledger (
                    facility_id, old_faction_id, new_faction_id, outfit_id, world_id, zone_id, players, duration_held, timestamp, zone_state
                ) VALUES (
                    @FacilityID, @OldFactionID, @NewFactionID, @OutfitID, @WorldID, @ZoneID, @Players, @DurationHeld, @Timestamp, @ZoneState
                );
            ");

            if (ev.Players == 0) {
                _Logger.LogWarning($"Have a count of 0 players in {ev}");
            }

            cmd.AddParameter("FacilityID", ev.FacilityID);
            cmd.AddParameter("OldFactionID", ev.OldFactionID);
            cmd.AddParameter("NewFactionID", ev.NewFactionID);
            cmd.AddParameter("OutfitID", ev.OutfitID);
            cmd.AddParameter("WorldID", ev.WorldID);
            cmd.AddParameter("ZoneID", ev.ZoneID);
            cmd.AddParameter("Players", ev.Players);
            cmd.AddParameter("DurationHeld", ev.DurationHeld);
            cmd.AddParameter("Timestamp", ev.Timestamp);
            cmd.AddParameter("ZoneState", (int?)ev.UnstableState);

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

    }
}
