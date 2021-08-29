using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models.Census;

namespace watchtower.Services.Census {

    public interface IMapCollection {

        Task<List<PsMap>> GetZoneMap(short worldID, uint zoneID);

    }

    public static class IMapCollectionExtensions {

        public static async Task<short?> GetZoneMapOwner(this IMapCollection census, short worldID, uint zoneID) {
            List<PsMap> map = await census.GetZoneMap(worldID, zoneID);

            int total = map.Count;

            Dictionary<short, int> counts = new();

            foreach (PsMap region in map) {
                if (counts.ContainsKey(region.FactionID) == false) {
                    counts.Add(region.FactionID, 0);
                }

                ++counts[region.FactionID];
            }

            if (total > 10 && counts.Count > 0) {
                KeyValuePair<short, int> majority = counts.ToList().OrderByDescending(iter => iter.Value).Last();

                if (majority.Value >= total - 1) {
                    return majority.Key;
                }
            }

            return null;
        }

    }

}
