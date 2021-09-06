using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Code.Constants;
using watchtower.Models.Census;

namespace watchtower.Services.Census {

    public interface IMapCollection {

        Task<List<PsMap>> GetZoneMap(short worldID, uint zoneID);

    }

    public static class IMapCollectionExtensions {

        /// <summary>
        /// Get the faction ID that owns a zone, or null if no owner
        /// </summary>
        /// <param name="census">Extension instance</param>
        /// <param name="worldID">World ID to get the zone owner of</param>
        /// <param name="zoneID">The zone ID</param>
        /// <returns>The faction ID that owns the zone, or null if there is no owner, or the map could not be found</returns>
        public static async Task<short?> GetZoneMapOwner(this IMapCollection census, short worldID, uint zoneID) {
            List<PsMap> map = await census.GetZoneMap(worldID, zoneID);

            return census._GetZoneMapOwner(worldID, zoneID, map);
        }

        /// <summary>
        /// Get who the owner of a zone is, based on list of regions and their owners
        /// </summary>
        /// <param name="census"></param>
        /// <param name="worldID"></param>
        /// <param name="zoneID"></param>
        /// <param name="map"></param>
        /// <returns></returns>
        private static short? _GetZoneMapOwner(this IMapCollection census, short worldID, uint zoneID, List<PsMap> map) {
            int total = map.Count;

            Dictionary<short, int> counts = new();

            foreach (PsMap region in map) {
                if (counts.ContainsKey(region.FactionID) == false) {
                    counts.Add(region.FactionID, 0);
                }

                ++counts[region.FactionID];
            }

            if (total > 10 && counts.Count > 0) {
                KeyValuePair<short, int> majority = counts.ToList().OrderByDescending(iter => iter.Value).First();

                // Esamir has 2 disabled regions
                if (majority.Value >= total - 2) {
                    return majority.Key;
                }
            }

            return null;
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
        public static async Task<UnstableState> GetUnstableState(this IMapCollection census, short worldID, uint zoneID) {
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

                if (value > (total / 3)) {
                    return UnstableState.DOUBLE_LANE;
                }
            }

            if (_GetZoneMapOwner(census, worldID, zoneID, map) == null) {
                return UnstableState.UNLOCKED;
            }

            return UnstableState.LOCKED;
        }

    }

}
