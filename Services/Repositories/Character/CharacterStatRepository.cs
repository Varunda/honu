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

    public class CharacterStatRepository {

        private readonly ILogger<CharacterStatRepository> _Logger;

        private readonly CharacterStatCollection _Census;
        private readonly CharacterStatDbStore _Db;

        private readonly IMemoryCache _Cache;
        private const string CACHE_KEY = "CharacterStat.{0}"; // {0} => Character stat

        public CharacterStatRepository(ILogger<CharacterStatRepository> logger,
            CharacterStatCollection census, CharacterStatDbStore db,
            IMemoryCache cache) {

            _Logger = logger;

            _Census = census;
            _Db = db;
            _Cache = cache;
        }

        /// <summary>
        ///     Get the <see cref="PsCharacterStat"/>s of a character
        /// </summary>
        /// <param name="charID">ID of the character</param>
        public async Task<List<PsCharacterStat>> GetByCharacterID(string charID) {
            string cacheKey = string.Format(CACHE_KEY, charID);

            if (_Cache.TryGetValue(cacheKey, out List<PsCharacterStat> stats) == false) {
                stats = await _Db.GetByID(charID);

                bool getCensus = (stats.Count == 0);
                if (getCensus == false) {
                    DateTime max = stats.Max(iter => iter.Timestamp);

                    if (DateTime.UtcNow - max > TimeSpan.FromDays(1)) {
                        getCensus = true;
                    }
                }

                if (getCensus == true) {
                    stats = await _Census.GetByID(charID);
                    await _Db.Set(charID, stats);
                }

                _Cache.Set(cacheKey, stats, new MemoryCacheEntryOptions() {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
                });
            }

            return stats;
        }

        public async Task Set(string charID, List<PsCharacterStat> stats) {
            if (stats.Count <= 0) {
                return;
            }

            string cacheKey = string.Format(CACHE_KEY, charID);
            _Cache.Remove(cacheKey);

            await _Db.Set(charID, stats);
        }

    }
}
