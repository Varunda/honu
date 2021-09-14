using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Census;

namespace watchtower.Services.Db.Readers {

    public class PsMapHexReader : IDataReader<PsMapHex> {

        public override PsMapHex ReadEntry(NpgsqlDataReader reader) {
            PsMapHex hex = new PsMapHex();

            hex.RegionID = reader.GetInt32("region_id");
            hex.HexType = reader.GetInt32("type_id");
            hex.ZoneID = reader.GetUInt32("zone_id");
            hex.X = reader.GetInt32("location_x");
            hex.Y = reader.GetInt32("location_y");

            return hex;
        }

    }
}
