using DaybreakGames.Census;
using DaybreakGames.Census.Operators;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Code.Constants;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Census;

namespace watchtower.Services.Census {

    public class MapCollection {

        private readonly ILogger<MapCollection> _Logger;
        private readonly ICensusQueryFactory _Census;
        //private readonly HonuCensus _HCensus;

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

            _Logger.LogDebug($"Census endpoint to get zones {zoneIDs} of {worldID}: {query.GetUri()}");

            List<PsMap> regions = new List<PsMap>();

            try {
                IEnumerable<JToken> result = await query.GetListAsync();

                foreach (JToken zone in result) {
                    uint zoneID = zone.GetUInt32("ZoneId");

                    //_Logger.LogDebug($"Zone data of {zoneID} => {zone}");

                    JToken? row = zone.SelectToken("Regions")?.SelectToken("Row");

                    if (row != null) {
                        foreach (JToken entry in row) {
                            JToken? data = entry.SelectToken("RowData");
                            if (data != null) {
                                PsMap region = _Parse(data);
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
            } catch (TaskCanceledException) {
                _Logger.LogInformation($"Cancelled task for getting regions for {worldID} [{string.Join(", ", zoneIDs)}]");
            } catch (Exception ex) {
                _Logger.LogError(ex, "Failed to get regions for {worldID} [{zoneID}]", worldID, string.Join(", ", zoneIDs));
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

            try {
                JToken? result = await query.GetAsync();

                if (result != null) {
                    JToken? row = result.SelectToken("Regions")?.SelectToken("Row");

                    if (row != null) {
                        foreach (JToken entry in row) {
                            JToken? data = entry.SelectToken("RowData");
                            if (data != null) {
                                PsMap region = _Parse(data);
                                region.ZoneID = zoneID; // Yeah we do a bit of cheating here
                                regions.Add(region);
                            }
                        }
                    } else {
                        _Logger.LogWarning($"Missing Regions?.Row?");
                    }
                }
            } catch (TaskCanceledException) {
                _Logger.LogInformation($"Cancelled task for getting regions for {worldID} {zoneID}");
            } catch (Exception ex) {
                _Logger.LogError(ex, $"Failed to get regions for {worldID} {zoneID}");
            }

            return regions;
        }

        /// <summary>
        ///     Get all map hexes from census
        /// </summary>
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
            } catch (TaskCanceledException) {

            } catch (Exception ex) {
                _Logger.LogError(ex, $"Failed to get all map hexes");
            }

            return hexes;
        }

        /// <summary>
        ///     Get all facility links from census
        /// </summary>
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

        /// <summary>
        /// Get who the owner of a zone is, based on list of regions and their owners
        /// </summary>
        /// <param name="worldID"></param>
        /// <param name="zoneID"></param>
        /// <param name="map"></param>
        /// <returns></returns>
        public short? GetZoneMapOwner(short worldID, uint zoneID, List<PsMap> map) {
            //_Logger.LogDebug($"{worldID}:{zoneID} => using {map.Count} regions");
            int total = map.Count;

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

            foreach (PsMap region in map) {
                if (counts.ContainsKey(region.FactionID) == false) {
                    counts.Add(region.FactionID, 0);
                }

                ++counts[region.FactionID];
            }

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
