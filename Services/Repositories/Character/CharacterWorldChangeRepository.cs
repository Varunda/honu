using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using watchtower.Models.Db;
using watchtower.Services.Db.Character;

namespace watchtower.Services.Repositories.Character {

    public class CharacterWorldChangeRepository {

        private readonly ILogger<CharacterWorldChangeRepository> _Logger;
        private readonly CharacterWorldChangeDbStore _Db;
        private readonly IMemoryCache _Cache;

        private const string CACHE_KEY = "CharacterWorldChange.{0}"; // {0} => char ID

        public CharacterWorldChangeRepository(ILogger<CharacterWorldChangeRepository> logger,
            CharacterWorldChangeDbStore db, IMemoryCache cache) {

            _Logger = logger;
            _Db = db;
            _Cache = cache;
        }

        /// <summary>
        ///     get all <see cref="WorldChange"/> for a character
        /// </summary>
        /// <param name="charID"></param>
        /// <returns></returns>
        public async Task<List<WorldChange>> GetByCharacterID(string charID) {
            string cacheKey = string.Format(CACHE_KEY, charID);

            if (_Cache.TryGetValue(cacheKey, out List<WorldChange>? changes) == false || changes == null) {
                changes = await _Db.GetByCharacterID(charID);

                _Cache.Set(cacheKey, changes, new MemoryCacheEntryOptions() {
                    SlidingExpiration = TimeSpan.FromMinutes(5)
                });
            }

            return changes;
        }

    }
}
