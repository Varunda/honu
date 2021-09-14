using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Census;

namespace watchtower.Services.Db.Implementations {

    public class FacilityDbStore : IDataReader<PsFacility>, IFacilityDbStore {

        private readonly ILogger<FacilityDbStore> _Logger;
        private readonly IDbHelper _DbHelper;

        public FacilityDbStore(ILogger<FacilityDbStore> logger,
            IDbHelper dbHelper) {

            _Logger = logger;
            _DbHelper = dbHelper;
        }

        public async Task<PsFacility?> GetByFacilityID(int facilityID) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM wt_facility
                    WHERE facility_id = @FacilityID;
            ");

            cmd.AddParameter("FacilityID", facilityID);

            PsFacility? fac = await ReadSingle(cmd);
            await conn.CloseAsync();

            return fac;
        }

        public async Task<List<PsFacility>> GetAll() {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM wt_facility;
            ");

            List<PsFacility> facilities = await ReadList(cmd);
            await conn.CloseAsync();

            return facilities;
        }

        public async Task Upsert(int facilityID, PsFacility facility) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                INSERT INTO wt_facility (
                    id, zone_id, region_id, name, type_id, type_name, location_x, location_y, location_z
                ) VALUES (
                    @ID, @ZoneID, @RegionID, @Name, @TypeID, @TypeName, @LocationX, @LocationY, @LocationZ
                ) ON CONFLICT (id) DO
                    UPDATE SET zone_id = @ZoneID,
                        region_id = @RegionID,
                        name = @Name,
                        type_id = @TypeID,
                        type_name = @TypeName,
                        location_x = @LocationX,
                        location_y = @LocationY,
                        location_z = @LocationZ
            ");

            cmd.AddParameter("ID", facilityID);
            cmd.AddParameter("ZoneID", facility.ZoneID);
            cmd.AddParameter("RegionID", facility.RegionID);
            cmd.AddParameter("Name", facility.Name);
            cmd.AddParameter("TypeID", facility.TypeID);
            cmd.AddParameter("TypeName", facility.TypeName);
            cmd.AddParameter("LocationX", facility.LocationX);
            cmd.AddParameter("LocationY", facility.LocationY);
            cmd.AddParameter("LocationZ", facility.LocationZ);

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

        public override PsFacility ReadEntry(NpgsqlDataReader reader) {
            PsFacility fac = new PsFacility();

            fac.FacilityID = reader.GetInt32("id");
            fac.ZoneID = reader.GetUInt32("zone_id");
            fac.RegionID = reader.GetInt32("region_id");
            fac.Name = reader.GetString("name");
            fac.TypeID = reader.GetInt32("type_id");
            fac.TypeName = reader.GetString("type_name");
            fac.LocationX = reader.GetNullableDecimal("location_x");
            fac.LocationY = reader.GetNullableDecimal("location_y");
            fac.LocationZ = reader.GetNullableDecimal("location_z");

            return fac;
        }
    }
}
