using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models.Census;
using watchtower.Services.Census;
using watchtower.Services.Db;

namespace watchtower.Services.Repositories.Implementations {
    
    public class CharacterItemRepository : ICharacterItemRepository {

        private readonly ILogger<CharacterItemRepository> _Logger;

        private readonly ICharacterItemCollection _Census;
        private readonly ICharacterItemDbStore _Db;

        private readonly IMemoryCache _Cache;
        private const string CACHE_KEY = "CharacterItems.CharacterID.{0}"; // {0} => Character ID

        public CharacterItemRepository(ILogger<CharacterItemRepository> logger,
            ICharacterItemCollection census, ICharacterItemDbStore db,
            IMemoryCache cache) {

            _Logger = logger;

            _Census = census;
            _Db = db;

            _Cache = cache;
        }

        public async Task<List<CharacterItem>> GetByID(string charID) {
            string cacheKey = string.Format(CACHE_KEY, charID);

            if (_Cache.TryGetValue(cacheKey, out List<CharacterItem> items) == false) {
                items = await _Db.GetByID(charID);

                if (items.Count == 0) {
                    items = await _Census.GetByID(charID);

                    _Logger.LogDebug($"Loaded {items.Count} items from census for character {charID}");

                    if (items.Count > 0) {
                        await _Db.Set(charID, items);
                    }
                }

                _Cache.Set(cacheKey, items, new MemoryCacheEntryOptions() {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
                });
            }

            return items;
        }

    }
}
