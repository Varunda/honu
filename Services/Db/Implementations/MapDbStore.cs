using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Census;

namespace watchtower.Services.Db.Implementations {

    public class MapDbStore : IMapDbStore {

        private readonly ILogger<MapDbStore> _Logger;
        private readonly IDataReader<PsMapHex> _HexReader;
        private readonly IDataReader<PsFacilityLink> _LinkReader;
        private readonly IDbHelper _DbHelper;

        public MapDbStore(ILogger<MapDbStore> logger,
            IDataReader<PsMapHex> hexReader, IDataReader<PsFacilityLink> linkReader,
            IDbHelper dbHelper) {

            _Logger = logger;
            _HexReader = hexReader;
            _LinkReader = linkReader;
            _DbHelper = dbHelper;
        }

        public async Task<List<PsFacilityLink>> GetFacilityLinks() {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM ledger_map_link;
            ");

            List<PsFacilityLink> links = await _LinkReader.ReadList(cmd);
            await conn.CloseAsync();

            return links;
        }

        public async Task<List<PsMapHex>> GetHexes() {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM ledger_map_hex;
            ");

            List<PsMapHex> hexes = await _HexReader.ReadList(cmd);
            await conn.CloseAsync();

            return hexes;
        }

        public async Task UpsertHex(PsMapHex hex) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                INSERT INTO ledger_map_hex (
                    region_id, zone_id, location_x, location_y, type_id
                ) VALUES (
                    @RegionID, @ZoneID, @LocationX, @LocationY, @TypeID
                ) ON CONFLICT(zone_id, location_x, location_y) 
                    DO UPDATE SET
                        region_id = @RegionID,
                        type_id = @TypeID;
            ");

            cmd.AddParameter("RegionID", hex.RegionID);
            cmd.AddParameter("ZoneID", hex.ZoneID);
            cmd.AddParameter("LocationX", hex.X);
            cmd.AddParameter("LocationY", hex.Y);
            cmd.AddParameter("TypeID", hex.HexType);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task UpsertLink(PsFacilityLink link) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                INSERT INTO ledger_map_link (
                    facility_a, facility_b, zone_id, description
                ) VALUES (
                    @FacilityA, @FacilityB, @ZoneID, @Description
                ) ON CONFLICT(facility_a, facility_b) 
                    DO UPDATE SET
                        zone_id = @ZoneID,
                        description = @Description;
            ");

            cmd.AddParameter("FacilityA", link.FacilityA);
            cmd.AddParameter("FacilityB", link.FacilityB);
            cmd.AddParameter("ZoneID", link.ZoneID);
            cmd.AddParameter("Description", link.Description);

            await cmd.ExecuteNonQueryAsync();
        }

    }
}
