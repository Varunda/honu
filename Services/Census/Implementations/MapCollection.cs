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

    public class MapCollection : IMapCollection {

        private readonly ILogger<MapCollection> _Logger;

        private readonly ICensusQueryFactory _Census;

        public MapCollection(ILogger<MapCollection> logger,
            ICensusQueryFactory census) {

            _Logger = logger;
            _Census = census;
        }

        public async Task<List<PsMap>> GetZoneMap(short worldID, int zoneID) {
            CensusQuery query = _Census.Create("map");
            query.Where("world_id").Equals(worldID);
            query.Where("zone_ids").Equals(zoneID);

            List<PsMap> regions = new List<PsMap>();

            try {
                JToken? result = await query.GetAsync();

                if (result != null) {
                    JToken? row = result.SelectToken("Regions")?.SelectToken("Row");

                    if (row != null) {
                        foreach (JToken entry in row) {
                            JToken? data = entry.SelectToken("RowData");
                            if (data != null) {
                                PsMap region = _Parse(data);
                                regions.Add(region);
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                _Logger.LogError(ex, "Failed to get regions for {worldID} {zoneID}", worldID, zoneID);
            }

            return regions;
        }

        private PsMap _Parse(JToken token) {
            PsMap region = new PsMap();

            region.RegionID = token.GetString("RegionId", "");
            region.FactionID = token.GetInt16("FactionId", -1);

            return region;
        }

    }
}
