using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Events;

namespace watchtower.Services.Db.Implementations {

    public class FacilityControlDbStore : IFacilityControlDbStore {

        private readonly ILogger<FacilityControlDbStore> _Logger;
        private readonly IDbHelper _DbHelper;

        public FacilityControlDbStore(ILogger<FacilityControlDbStore> logger,
            IDbHelper helper) {

            _Logger = logger;
            _DbHelper = helper;
        }

        public async Task Insert(FacilityControlEvent ev) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                INSERT INTO wt_ledger (
                    facility_id, old_faction_id, new_faction_id, outfit_id, world_id, zone_id, players, duration_held, timestamp
                ) VALUES (
                    @FacilityID, @OldFactionID, @NewFactionID, @OutfitID, @WorldID, @ZoneID, @Players, @DurationHeld, @Timestamp
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

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

    }
}
