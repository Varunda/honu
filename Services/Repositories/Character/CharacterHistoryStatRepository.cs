using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models.Census;
using watchtower.Services.Census;
using watchtower.Services.Db;

namespace watchtower.Services.Repositories {

    public class CharacterHistoryStatRepository {

        private readonly ILogger<CharacterHistoryStatRepository> _Logger;
        private readonly IMemoryCache _Cache;

        private const string CACHE_KEY = "CharacterHistoryStat.{0}"; // {0} => Char ID

        private readonly CharacterHistoryStatCollection _Census;
        private readonly CharacterHistoryStatDbStore _Db;

        public CharacterHistoryStatRepository(ILogger<CharacterHistoryStatRepository> logger,
            IMemoryCache cache,
            CharacterHistoryStatCollection census, CharacterHistoryStatDbStore db) {

            _Logger = logger;
            _Cache = cache;

            _Census = census ?? throw new ArgumentNullException(nameof(census));
            _Db = db ?? throw new ArgumentNullException(nameof(db));
        }

        /// <summary>
        ///     Get the <see cref="PsCharacterHistoryStat"/>s for a character
        /// </summary>
        /// <param name="charID">ID of the character</param>
        /// <returns>
        ///     All <see cref="PsCharacterHistoryStat"/> with <see cref="PsCharacterHistoryStat.CharacterID"/> of <paramref name="charID"/>
        /// </returns>
        public async Task<List<PsCharacterHistoryStat>> GetByCharacterID(string charID) {
            string cacheKey = string.Format(CACHE_KEY, charID);

            if (_Cache.TryGetValue(cacheKey, out List<PsCharacterHistoryStat> stats) == false) {
                stats = await _Db.GetByCharacterID(charID);

                bool getFromCensus = stats.Count == 0;

                if (getFromCensus == false) {
                    // Some stats, such as battle rank, are not updated everytime, cause why would BR need
                    //      to be updated if it's already at max rank
                    DateTime mostRecent = stats.Max(iter => iter.LastUpdated);
                    //_Logger.LogTrace($"Most recent history stat for {charID} is from {mostRecent}");

                    getFromCensus = DateTime.UtcNow - mostRecent > TimeSpan.FromDays(1);
                }

                if (getFromCensus == true) {
                    //_Logger.LogTrace($"Getting history stats for {charID} from census");
                    List<PsCharacterHistoryStat> census = await _Census.GetByCharacterID(charID);

                    if (census.Count > 0) {
                        foreach (PsCharacterHistoryStat stat in census) {
                            PsCharacterHistoryStat? db = stats.FirstOrDefault(iter => iter.Type == stat.Type);

                            if (db == null || stat.LastUpdated > db.LastUpdated) {
                                //_Logger.LogTrace($"Stat for type {stat.Type} does not exist in DB, or is outdated ({db?.LastUpdated})");
                                await _Db.Upsert(charID, stat.Type, stat);
                            }
                        }

                        stats = census;
                    }
                }

                _Cache.Set(cacheKey, stats, new MemoryCacheEntryOptions() {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(20)
                });
            }

            return stats;
        }

        /// <summary>
        ///     Update/Insert (update) a <see cref="PsCharacterHistoryStat"/>
        /// </summary>
        /// <param name="charID">Character ID</param>
        /// <param name="type">Type</param>
        /// <param name="stat">Parameters</param>
        /// <returns>
        ///     A task when the operation is complete
        /// </returns>
        public Task Upsert(string charID, string type, PsCharacterHistoryStat stat) {
            string cacheKey = string.Format(CACHE_KEY, charID);
            _Cache.Remove(cacheKey);

            return _Db.Upsert(charID, type, stat);
        }

    }
}
