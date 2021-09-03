using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models.Db;

namespace watchtower.Services.Db.Readers {

    public class FacilityControlEntryReader : IDataReader<FacilityControlDbEntry> {

        public override FacilityControlDbEntry ReadEntry(NpgsqlDataReader reader) {
            FacilityControlDbEntry entry = new FacilityControlDbEntry();

            entry.FacilityID = reader.GetInt32("facility_id");
            entry.Captured = reader.GetInt32("captured");
            entry.CaptureAverage = reader.GetFloat("capture_average");
            entry.Defended = reader.GetInt32("defended");
            entry.DefenseAverage = reader.GetFloat("defend_average");
            entry.TotalAverage = reader.GetFloat("total_average");

            return entry;

        }
    }
}
