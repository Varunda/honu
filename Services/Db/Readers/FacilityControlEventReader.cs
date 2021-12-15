using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Events;

namespace watchtower.Services.Db.Readers {

    public class FacilityControlEventReader : IDataReader<FacilityControlEvent> {

        public override FacilityControlEvent ReadEntry(NpgsqlDataReader reader) {
            FacilityControlEvent ev = new FacilityControlEvent();

            ev.ID = reader.GetInt64("id");
            ev.FacilityID = reader.GetInt32("facility_id");
            ev.OldFactionID = reader.GetInt16("old_faction_id");
            ev.NewFactionID = reader.GetInt16("new_faction_id");
            ev.OutfitID = reader.GetNullableString("outfit_id");
            ev.WorldID = reader.GetInt16("world_id");
            ev.ZoneID = reader.GetUInt32("zone_id");
            ev.Players = reader.GetInt32("players");
            ev.DurationHeld = reader.GetInt32("duration_held");
            ev.Timestamp = reader.GetDateTime("timestamp");

            return ev;
        }

    }
}
