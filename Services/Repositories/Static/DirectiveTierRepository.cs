using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using watchtower.Models.Census;
using watchtower.Models.Db;
using watchtower.Services.Census;
using watchtower.Services.Db;

namespace watchtower.Services.Repositories {

    public class DirectiveTierRepository {

        private readonly ILogger<DirectiveTierRepository> _Logger;
        private readonly IMemoryCache _Cache;

        private readonly DirectiveTierDbStore _Db;

        private const string CACHE_KEY = "DirectiveTiers.All"; 

        public DirectiveTierRepository(ILogger<DirectiveTierRepository> logger,
            IMemoryCache cache, DirectiveTierDbStore db) {

            _Logger = logger;
            _Cache = cache;

            _Db = db;
        }

        public async Task<List<DirectiveTier>> GetAll() {
            if (_Cache.TryGetValue(CACHE_KEY, out List<DirectiveTier>? dirs) == false || dirs == null) {
                dirs = await _Db.GetAll();

                if (dirs.Count > 0) {
                    _Cache.Set(CACHE_KEY, dirs, new MemoryCacheEntryOptions() {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(2)
                    });
                }
            }

            return dirs;
        }

        public async Task<DirectiveTier?> GetByTierAndTree(int tierID, int treeID) {
            return (await GetAll()).FirstOrDefault(iter => iter.TierID == tierID && iter.TreeID == treeID);
        }

    }

}