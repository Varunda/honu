using DaybreakGames.Census;
using DaybreakGames.Census.Operators;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Census;

namespace watchtower.Services.Census.Implementations {

    public class FacilityCollection : IFacilityCollection {

        private readonly ILogger<FacilityCollection> _Logger;
        private readonly ICensusQueryFactory _Census;

        public FacilityCollection(ILogger<FacilityCollection> logger,
            ICensusQueryFactory census) {

            _Logger = logger;
            _Census = census;
        }

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

            return facilities;
        }

        public Task<PsFacility?> GetByFacilityID(int facilityID) {
            throw new NotImplementedException();
        }

        private PsFacility _Parse(JToken token) {
            PsFacility facility = new PsFacility();
            facility.FacilityID = token.GetInt32("facility_id", 0);
            facility.RegionID = token.GetInt32("region_id", 0);
            facility.ZoneID = token.GetZoneID();
            facility.Name = token.GetString("facility_name", "<missing name>");
            facility.TypeID = token.GetInt32("facility_type_id", 0);
            facility.TypeName = token.GetString("facility_type", "<missing type name>");

            return facility;
        }

    }
}
