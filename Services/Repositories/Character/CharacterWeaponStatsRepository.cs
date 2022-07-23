using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models.CharacterViewer.WeaponStats;
using watchtower.Services.Census;
using watchtower.Services.Db;

namespace watchtower.Services.Repositories {

    public class CharacterWeaponStatRepository {

        private readonly ILogger<CharacterWeaponStatRepository> _Logger;
        private readonly IMemoryCache _Cache;

        private const string CACHE_KEY = "CharacterWeaponStats.{0}"; // {0} => Character ID

        private readonly CharacterWeaponStatCollection _Census;
        private readonly CharacterWeaponStatDbStore _Db;

        public CharacterWeaponStatRepository(ILogger<CharacterWeaponStatRepository> logger,
            IMemoryCache cache,
            CharacterWeaponStatCollection coll, CharacterWeaponStatDbStore db) {

            _Logger = logger;
            _Cache = cache;

            _Census = coll ?? throw new ArgumentNullException(nameof(coll));
            _Db = db ?? throw new ArgumentNullException(nameof(db));
        }

        /// <summary>
        ///     Get the <see cref="WeaponStatEntry"/>s for a character
        /// </summary>
        /// <param name="charID">ID of the character</param>
        /// <returns>
        ///     A <see cref="WeaponStatEntry"/> with <see cref="WeaponStatEntry.CharacterID"/> of <paramref name="charID"/>
        /// </returns>
        public async Task<List<WeaponStatEntry>> GetByCharacterID(string charID) {
            string cacheKey = string.Format(CACHE_KEY, charID);

            if (_Cache.TryGetValue(cacheKey, out List<WeaponStatEntry> entries) == false) {
                entries = await _Db.GetByCharacterID(charID);

                if (entries.Count == 0) {
                    entries = await _Census.GetByCharacterID(charID);

                    foreach (WeaponStatEntry entry in entries) {
                        await _Db.Upsert(entry);
                    }
                }

                _Cache.Set(cacheKey, entries, new MemoryCacheEntryOptions() {
                    SlidingExpiration = TimeSpan.FromMinutes(30)
                });
            }

            return entries;
        }
    }

}
