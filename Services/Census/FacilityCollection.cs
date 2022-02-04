using DaybreakGames.Census;
using DaybreakGames.Census.Operators;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Census;

namespace watchtower.Services.Census {

    /// <summary>
    ///     Service to interact with the /map_region collection
    /// </summary>
    public class FacilityCollection {

        private readonly ILogger<FacilityCollection> _Logger;
        private readonly ICensusQueryFactory _Census;

        private const string PATCH_FILE = "./census-patches/map_region.json";

        public FacilityCollection(ILogger<FacilityCollection> logger,
            ICensusQueryFactory census) {

            _Logger = logger;
            _Census = census;
        }

        /// <summary>
        ///     Get all <see cref="PsFacility"/>s currently in Census
        /// </summary>
        /// <returns></returns>
        public async Task<List<PsFacility>> GetAll() {
            CensusQuery query = _Census.Create("map_region");
            query.SetLimit(1000);

            List<PsFacility> facilities = new List<PsFacility>();

            try {
                IEnumerable<JToken> results = await query.GetListAsync();

                foreach (JToken token in results) {
                    // Some regions, such as The Wash and the Shattered Warpgate, don't have a facility in the region
                    if (token.Value<string?>("facility_id") == null) {
                        continue;
                    }

                    PsFacility facility = _Parse(token);
                    facilities.Add(facility);
                }

            } catch (Exception ex) {
                _Logger.LogError(ex, "Failed to get all");
            }

            try {
                do {
                    string patch = File.ReadAllText(PATCH_FILE);

                    JToken json = JToken.Parse(patch);

                    JToken? mapRegionList = json.SelectToken("map_region_list");
                    if (mapRegionList == null) {
                        _Logger.LogError($"Missing token 'map_region_list' from {PATCH_FILE}");
                        break;
                    }

                    IEnumerable<JToken> arr = mapRegionList.Children();

                    foreach (JToken token in arr) {
                        string? facilityID = token.Value<string?>("facility_id");
                        if (facilityID == null || facilityID == "0") {
                            continue;
                        }

                        PsFacility fac = _Parse(token);
                        facilities.Add(fac);
                        //_Logger.LogDebug($"Parsed facility: {fac.Name} => {JToken.FromObject(fac)}");
                    }

                    _Logger.LogInformation($"Found {arr.Count()} entries in map_region_list object");
                } while (false);
            } catch (Exception ex) {
                _Logger.LogError(ex, "failed to patch map_region");
            }

            return facilities;
        }

        private PsFacility _Parse(JToken token) {
            PsFacility facility = new PsFacility();

            facility.FacilityID = token.GetInt32("facility_id", 0);
            facility.RegionID = token.GetInt32("map_region_id", 0);
            facility.ZoneID = token.GetZoneID();
            facility.Name = token.GetString("facility_name", "<missing name>");
            facility.TypeID = token.GetInt32("facility_type_id", 0);
            facility.TypeName = token.GetString("facility_type", "<missing type name>");
            facility.LocationX = token.Value<decimal?>("location_x");
            facility.LocationZ = token.Value<decimal?>("location_z");
            facility.LocationY = token.Value<decimal?>("location_y");

            // The map_region patch has null for the Y coord, but has X//Z coords
            if (facility.LocationX != null && facility.LocationZ != null && facility.LocationY == null) {
                facility.LocationY = 0;
            }
            return facility;
        }

    }
}
