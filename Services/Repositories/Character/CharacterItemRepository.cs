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

    public class CharacterItemRepository {

        private readonly ILogger<CharacterItemRepository> _Logger;

        private readonly CharacterItemCollection _Census;
        private readonly CharacterItemDbStore _Db;

        private readonly IMemoryCache _Cache;
        private const string CACHE_KEY = "CharacterItems.CharacterID.{0}"; // {0} => Character ID

        public CharacterItemRepository(ILogger<CharacterItemRepository> logger,
            CharacterItemCollection census, CharacterItemDbStore db,
            IMemoryCache cache) {

            _Logger = logger;

            _Census = census;
            _Db = db;

            _Cache = cache;
        }

        /// <summary>
        ///     Get the <see cref="CharacterItem"/>s of a charcter
        /// </summary>
        /// <param name="charID">ID of the character</param>
        /// <returns></returns>
        public async Task<List<CharacterItem>> GetByID(string charID) {
            string cacheKey = string.Format(CACHE_KEY, charID);

            if (_Cache.TryGetValue(cacheKey, out List<CharacterItem>? items) == false || items == null) {
                items = await _Db.GetByID(charID);

                if (items.Count == 0) {
                    items = await _Census.GetByID(charID);

                    _Logger.LogDebug($"Loaded {items.Count} items from census for character {charID}");

                    foreach (CharacterItem i in items) {
                        await _Db.Upsert(i);
                    }
                }

                _Cache.Set(cacheKey, items, new MemoryCacheEntryOptions() {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
                });
            }

            return items;
        }

        /// <summary>
        ///     Upsert a single <see cref="CharacterItem"/>
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public async Task Upsert(CharacterItem item) {
            string cacheKey = string.Format(CACHE_KEY, item.CharacterID);
            _Cache.Remove(cacheKey);

            await _Db.Upsert(item);
        }

    }
}
