using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Census;

namespace watchtower.Services.Db.Readers {

    public class PsFacilityLinkReader : IDataReader<PsFacilityLink> {

        public override PsFacilityLink ReadEntry(NpgsqlDataReader reader) {
            PsFacilityLink link = new PsFacilityLink();

            link.FacilityA = reader.GetInt32("facility_a");
            link.FacilityB = reader.GetInt32("facility_b");
            link.ZoneID = reader.GetUInt32("zone_id");
            link.Description = reader.GetNullableString("description");

            return link;
        }

    }
}
