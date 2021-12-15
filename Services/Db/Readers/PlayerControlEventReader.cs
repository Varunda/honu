using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Events;

namespace watchtower.Services.Db.Readers {

    public class PlayerControlEventReader : IDataReader<PlayerControlEvent> {

        public override PlayerControlEvent? ReadEntry(NpgsqlDataReader reader) {
            PlayerControlEvent ev = new PlayerControlEvent();

            ev.ControlID = reader.GetInt64("control_id");
            ev.CharacterID = reader.GetString("character_id");
            ev.FacilityID = reader.GetInt32("facility_id");
            ev.OutfitID = reader.GetNullableString("outfit_id");
            ev.Timestamp = reader.GetDateTime("timestamp");
            ev.WorldID = reader.GetInt16("world_id");
            ev.ZoneID = reader.GetUInt32("zone_id");

            short oldFactionID = reader.GetInt16("old_faction_id");
            short newFactionID = reader.GetInt16("new_faction_id");

            ev.IsCapture = oldFactionID != newFactionID;

            return ev;
        }

    }
}
