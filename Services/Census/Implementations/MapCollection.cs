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

        public async Task<List<PsMap>> GetZoneMap(short worldID, uint zoneID) {
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

        public async Task<List<PsMapHex>> GetHexes() {
            CensusQuery query = _Census.Create("map_hex");
            query.SetLimit(10000);

            List<PsMapHex> hexes = new List<PsMapHex>();

            try {
                IEnumerable<JToken> result = await query.GetListAsync();

                foreach (JToken token in result) {
                    PsMapHex hex = _ParseHex(token);
                    hexes.Add(hex);
                }
            } catch (Exception ex) {
                _Logger.LogError(ex, $"Failed to get all map hexes");
            }

            return hexes;
        }

        public async Task<List<PsFacilityLink>> GetFacilityLinks() {
            CensusQuery query = _Census.Create("facility_link");
            query.SetLimit(10000);

            List<PsFacilityLink> hexes = new List<PsFacilityLink>();

            try {
                IEnumerable<JToken> result = await query.GetListAsync();

                foreach (JToken token in result) {
                    PsFacilityLink hex = _ParseLink(token);
                    hexes.Add(hex);
                }
            } catch (Exception ex) {
                _Logger.LogError(ex, $"Failed to get facility links");
            }

            return hexes;
        }

        private PsMap _Parse(JToken token) {
            PsMap region = new PsMap();

            region.RegionID = token.GetString("RegionId", "");
            region.FactionID = token.GetInt16("FactionId", -1);

            return region;
        }

        private PsMapHex _ParseHex(JToken token) {
            PsMapHex hex = new PsMapHex();

            hex.ZoneID = token.GetZoneID();
            hex.RegionID = token.GetInt32("map_region_id", 0);
            hex.X = token.GetInt32("x", 0);
            hex.Y = token.GetInt32("y", 0);
            hex.HexType = token.GetInt32("hex_type", -1);

            return hex;
        }

        private PsFacilityLink _ParseLink(JToken token) {
            PsFacilityLink link = new PsFacilityLink();

            link.ZoneID = token.GetZoneID();
            link.FacilityA = token.GetInt32("facility_id_a", -1);
            link.FacilityB = token.GetInt32("facility_id_b", -1);
            link.Description = token.NullableString("description");

            return link;
        }

    }
}
