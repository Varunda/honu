using DaybreakGames.Census;
using DaybreakGames.Census.Operators;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using watchtower.Code.Constants;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Census;

namespace watchtower.Services.Census {

    public class MapCollection {

        internal readonly ILogger<MapCollection> _Logger;
        private readonly ICensusQueryFactory _Census;
        //private readonly HonuCensus _HCensus;

        private const string LINK_PATCH_FILE = "./census-patches/facility_link.json";

        public MapCollection(ILogger<MapCollection> logger,
            ICensusQueryFactory census) { //, HonuCensus hc) {

            _Logger = logger;
            _Census = census;

            //_HCensus = hc;
        }

        public async Task<List<PsMap>> GetZoneMaps(short worldID, List<uint> zoneIDs) {
            CensusQuery query = _Census.Create("map");
            query.Where("world_id").Equals(worldID);
            foreach (uint zoneID in zoneIDs) {
                query.Where("zone_ids").Equals(zoneID);
            }

            //_Logger.LogDebug($"Census endpoint to get zones {zoneIDs} of {worldID}: {query.GetUri()}");

            List<PsMap> regions = new List<PsMap>();

            IEnumerable<JsonElement> result = await query.GetListAsync();

            foreach (JsonElement zone in result) {
                uint zoneID = zone.GetUInt32("ZoneId");

                //_Logger.LogDebug($"Zone data of {zoneID} => {zone}");

                JsonElement? row = zone.GetChild("Regions")?.GetChild("Row");

                if (row != null) {
                    foreach (JsonElement entry in row.Value.EnumerateArray()) {
                        JsonElement? data = entry.GetChild("RowData");
                        if (data != null) {
                            PsMap region = _Parse(data.Value);
                            region.ZoneID = zoneID;
                            regions.Add(region);
                        } else {
                            _Logger.LogWarning($"Missing RowData from {entry}");
                        }
                    }
                } else {
                    _Logger.LogWarning($"Missing Regions?.Row?");
                }
            }

            return regions;
        }

        /// <summary>
        ///     Get who owns each base in a zone
        /// </summary>
        /// <param name="worldID">ID of the world</param>
        /// <param name="zoneID">ID of the zone</param>
        /// <returns>
        ///     A list containing <see cref="PsMap"/>s for each facility in the zone
        /// </returns>
        public async Task<List<PsMap>> GetZoneMap(short worldID, uint zoneID) {
            CensusQuery query = _Census.Create("map");
            query.Where("world_id").Equals(worldID);
            query.Where("zone_ids").Equals(zoneID);

            List<PsMap> regions = new List<PsMap>();

            JsonElement? result = await query.GetAsync();

            if (result != null) {
                JsonElement? row = result.Value.GetChild("Regions")?.GetChild("Row");

                if (row != null) {
                    foreach (JsonElement entry in row.Value.EnumerateArray()) {
                        JsonElement? data = entry.GetChild("RowData");
                        if (data != null) {
                            PsMap region = _Parse(data.Value);
                            region.ZoneID = zoneID; // Yeah we do a bit of cheating here
                            regions.Add(region);
                        }
                    }
                } else {
                    _Logger.LogWarning($"Missing Regions?.Row?");
                }
            }

            return regions;
        }

        /// <summary>
        ///     Get all map hexes from census
        /// </summary>
        public async Task<List<PsMapHex>> GetHexes() {
            CensusQuery query = _Census.Create("map_hex");
            query.SetLimit(5000);

            List<PsMapHex> hexes = new List<PsMapHex>();

            for (int i = 0; i < 10; ++i) {
                query.SetStart(i * 5000);

                IEnumerable<JsonElement> result = await query.GetListAsync();

                _Logger.LogDebug($"loaded {result.Count()} on iteration {i}");

                foreach (JsonElement token in result) {
                    PsMapHex hex = _ParseHex(token);
                    hexes.Add(hex);
                }

                if (result.Count() < 5000) {
                    break;
                }
            }

            return hexes;
        }

        /// <summary>
        ///     Get all facility links from census
        /// </summary>
        public async Task<List<PsFacilityLink>> GetFacilityLinks() {
            CensusQuery query = _Census.Create("facility_link");
            query.SetLimit(5000);

            List<PsFacilityLink> hexes = new List<PsFacilityLink>();

            IEnumerable<JsonElement> result = await query.GetListAsync();

            foreach (JsonElement token in result) {
                PsFacilityLink hex = _ParseLink(token);
                hexes.Add(hex);
            }

            /* Census updated :pogu:
            do {
                string patch = File.ReadAllText(LINK_PATCH_FILE);
                JToken json = JToken.Parse(patch);

                JToken? facilityLinkList = json.SelectToken("facility_link_list");
                if (facilityLinkList == null) {
                    _Logger.LogWarning($"Missing 'facility_link_list' from patch file in {LINK_PATCH_FILE}");
                    break;
                }

                IEnumerable<JToken> arr = facilityLinkList.Children();
                _Logger.LogInformation($"Have {arr.Count()} entries to patch into facility_link");
                foreach (JToken token in arr) {
                    PsFacilityLink hex = _ParseLink(token);
                    hexes.Add(hex);
                }
            } while (false);
            */

            return hexes;
        }

        /// <summary>
        /// Get who the owner of a zone is, based on list of regions and their owners
        /// </summary>
        /// <param name="worldID"></param>
        /// <param name="zoneID"></param>
        /// <param name="map"></param>
        /// <returns></returns>
        public short? GetZoneMapOwner(short worldID, uint zoneID, List<PsMap> map) {
            //_Logger.LogDebug($"{worldID}:{zoneID} => using {map.Count} regions");
            int total = map.Where(iter => iter.FactionID != 0).Count();

            Dictionary<short, int> counts = new();

            foreach (PsMap region in map) {
                if (counts.ContainsKey(region.FactionID) == false) {
                    counts.Add(region.FactionID, 0);
                }

                ++counts[region.FactionID];
            }

            //_Logger.LogInformation($"{worldID}:{zoneID} => {string.Join(", ", counts.Select(kvp => kvp.Key + ": " + kvp.Value))}");

            if (total > 10 && counts.Count > 0) {
                KeyValuePair<short, int> majority = counts.ToList().OrderByDescending(iter => iter.Value).First();

                // Esamir has 2 disabled regions
                if (majority.Value >= total - 2) {
                    return majority.Key;
                }
            }

            return null;
        }

        private PsMap _Parse(JsonElement token) {
            PsMap region = new PsMap();

            region.RegionID = token.GetString("RegionId", "");
            region.FactionID = token.GetInt16("FactionId", -1);

            return region;
        }

        private PsMapHex _ParseHex(JsonElement token) {
            PsMapHex hex = new PsMapHex();

            hex.ZoneID = token.GetZoneID();
            hex.RegionID = token.GetInt32("map_region_id", 0);
            hex.X = token.GetInt32("x", 0);
            hex.Y = token.GetInt32("y", 0);
            hex.HexType = token.GetInt32("hex_type", -1);

            return hex;
        }

        private PsFacilityLink _ParseLink(JsonElement token) {
            PsFacilityLink link = new PsFacilityLink();

            link.ZoneID = token.GetZoneID();
            link.FacilityA = token.GetInt32("facility_id_a", -1);
            link.FacilityB = token.GetInt32("facility_id_b", -1);
            link.Description = token.NullableString("description");

            return link;
        }

    }

    public static class IMapCollectionExtensions {

        /// <summary>
        /// Get the faction ID that owns a zone, or null if no owner
        /// </summary>
        /// <param name="census">Extension instance</param>
        /// <param name="worldID">World ID to get the zone owner of</param>
        /// <param name="zoneID">The zone ID</param>
        /// <returns>The faction ID that owns the zone, or null if there is no owner, or the map could not be found</returns>
        public static async Task<short?> GetZoneMapOwner(this MapCollection census, short worldID, uint zoneID) {
            List<PsMap> map = await census.GetZoneMap(worldID, zoneID);

            return census.GetZoneMapOwner(worldID, zoneID, map);
        }


        /// <summary>
        ///     Get what <c>UnstableState</c> a zone is
        /// </summary>
        /// <param name="census">Extension instance</param>
        /// <param name="worldID">World ID</param>
        /// <param name="zoneID">Zone ID</param>
        /// <returns>
        ///     The <see cref="UnstableState"/> of the zone. If no zone is found, <see cref="UnstableState.LOCKED"/> is returned
        /// </returns>
        public static async Task<UnstableState> GetUnstableState(this MapCollection census, short worldID, uint zoneID) {
            List<PsMap> map = await census.GetZoneMap(worldID, zoneID);

            int total = map.Count;

            Dictionary<short, int> counts = new();
            int found = 0;

            foreach (PsMap region in map) {
                if (counts.ContainsKey(region.FactionID) == false) {
                    counts.Add(region.FactionID, 0);
                }

                ++counts[region.FactionID];
                ++found;
            }

            //census._Logger.LogInformation($"GetUnstableState = {worldID}:{zoneID}, have {found} regions in zone => {string.Join(", ", counts.Select(kvp => kvp.Key + ": " + kvp.Value))}\n\t{string.Join(", ", map.Select(iter => iter.RegionID))}");

            if (counts.TryGetValue(0, out int value) == true) {
                if (value > (total / 2)) {
                    return UnstableState.SINGLE_LANE;
                }

                if (value > 10) {
                    return UnstableState.DOUBLE_LANE;
                }
            }

            if (census.GetZoneMapOwner(worldID, zoneID, map) == null) {
                return UnstableState.UNLOCKED;
            }

            return UnstableState.LOCKED;
        }

    }

}
