using Npgsql;
using System.Data;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Census;

namespace watchtower.Services.Db.Readers {

    public class AlertReader : IDataReader<PsAlert> {

        public override PsAlert? ReadEntry(NpgsqlDataReader reader) {
            PsAlert alert = new PsAlert();

            alert.ID = reader.GetInt64("id");
            alert.Timestamp = reader.GetDateTime("timestamp");
            alert.Duration = reader.GetInt32("duration");
            alert.ZoneID = reader.GetUInt32("zone_id");
            alert.WorldID = reader.GetInt16("world_id");
            alert.AlertID = reader.GetInt32("alert_id");
            alert.VictorFactionID = reader.GetNullableInt16("victor_faction_id");
            alert.WarpgateVS = reader.GetInt32("warpgate_vs");
            alert.WarpgateNC = reader.GetInt32("warpgate_nc");
            alert.WarpgateTR = reader.GetInt32("warpgate_tr");
            alert.ZoneFacilityCount = reader.GetInt32("zone_facility_count");
            alert.CountVS = reader.GetNullableInt32("count_vs");
            alert.CountNC = reader.GetNullableInt32("count_nc");
            alert.CountTR = reader.GetNullableInt32("count_tr");
            alert.Participants = reader.GetInt32("participants");

            return alert;
        }

    }
}
