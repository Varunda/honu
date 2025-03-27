using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using watchtower.Models.Api;
using watchtower.Services.Db;

namespace watchtower.Services.Repositories {

    public class NameFightRepository {

        private readonly ILogger<NameFightRepository> _Logger;
        private readonly NameFightDbStore _NameFightDb;
        private readonly IMemoryCache _Cache;

        private const string CACHE_KEY = "Honu.NameFight.Entry";

        public NameFightRepository(ILogger<NameFightRepository> logger,
            NameFightDbStore nameFightDb, IMemoryCache cache) {

            _Logger = logger;
            _NameFightDb = nameFightDb;
            _Cache = cache;
        }

        public async Task<List<NameFightEntry>> Get() {
            if (_Cache.TryGetValue(CACHE_KEY, out List<NameFightEntry>? entry) == false || entry == null) {
                entry = await _NameFightDb.Get();

                _Cache.Set(CACHE_KEY, entry, new MemoryCacheEntryOptions() {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(10)
                });
            }

            return entry;
        }

    }
}
