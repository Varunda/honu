using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Constants;
using watchtower.Models;
using watchtower.Models.Api;

namespace watchtower.Services.Repositories {

    public class WorldPopulationRepository {

        private readonly ILogger<WorldPopulationRepository> _Logger;
        private readonly IMemoryCache _Cache;

        private const string CACHE_KEY = "Population.{0}"; // {0} => World ID
        private const int CACHE_DURATION = 15; // seconds

        public WorldPopulationRepository(ILogger<WorldPopulationRepository> logger,
            IMemoryCache cache) {

            _Logger = logger;
            _Cache = cache;
        }

        public WorldPopulation GetByWorldID(short worldID) {
            string cacheKey = string.Format(CACHE_KEY, worldID);

            if (_Cache.TryGetValue(cacheKey, out WorldPopulation pop) == false) {
                Stopwatch timer = Stopwatch.StartNew();
                pop = new WorldPopulation();
                pop.Timestamp = DateTime.UtcNow;
                pop.CachedUntil = pop.Timestamp.AddSeconds(CACHE_DURATION);
                pop.WorldID = worldID;

                lock (CharacterStore.Get().Players) {
                    int Count(Func<KeyValuePair<string, TrackedPlayer>, bool> predicate) {
                        return CharacterStore.Get().Players.Count(predicate);
                    }

                    int totalCount = Count(iter => iter.Value.WorldID == worldID);

                    pop.Total = CharacterStore.Get().Players.Where(iter => iter.Value.WorldID == worldID && iter.Value.Online == true).Count();

                    pop.Vs = Count(iter => iter.Value.WorldID == worldID && iter.Value.FactionID == Faction.VS && iter.Value.Online == true);
                    pop.Nc = Count(iter => iter.Value.WorldID == worldID && iter.Value.FactionID == Faction.NC && iter.Value.Online == true);
                    pop.Tr = Count(iter => iter.Value.WorldID == worldID && iter.Value.FactionID == Faction.TR && iter.Value.Online == true);
                    pop.Ns = Count(iter => iter.Value.WorldID == worldID && iter.Value.FactionID == Faction.NS && iter.Value.Online == true);

                    pop.Ns_vs = Count(iter => iter.Value.WorldID == worldID && iter.Value.FactionID == Faction.NS && iter.Value.TeamID == Faction.VS && iter.Value.Online == true);
                    pop.Ns_nc = Count(iter => iter.Value.WorldID == worldID && iter.Value.FactionID == Faction.NS && iter.Value.TeamID == Faction.NC && iter.Value.Online == true);
                    pop.Ns_tr = Count(iter => iter.Value.WorldID == worldID && iter.Value.FactionID == Faction.NS && iter.Value.TeamID == Faction.TR && iter.Value.Online == true);
                    pop.NsOther = Count(iter => iter.Value.WorldID == worldID && iter.Value.FactionID == Faction.NS && iter.Value.TeamID == Faction.NS && iter.Value.Online == true);
                }

                _Logger.LogDebug($"Took {timer.ElapsedMilliseconds}ms to build world data for world ID {worldID}, "
                    + $"caching for {CACHE_DURATION} seconds (until {pop.CachedUntil:u})");

                _Cache.Set(cacheKey, pop, new MemoryCacheEntryOptions() {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(CACHE_DURATION)
                });
            }

            return pop;
        }

    }
}
