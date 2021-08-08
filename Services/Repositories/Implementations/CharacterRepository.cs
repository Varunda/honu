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

    public class CharacterRepository : ICharacterRepository {

        private readonly ILogger<CharacterRepository> _Logger;
        private readonly IMemoryCache _Cache;

        private readonly ICharacterDbStore _Db;
        private readonly ICharacterCollection _Census;

        private const string _CacheKeyID = "Character.ID.{0}"; // {0} => char ID
        private const string _CacheKeyName = "Character.Name.{0}"; // {0} => char ID

        public CharacterRepository(ILogger<CharacterRepository> logger, IMemoryCache cache,
                ICharacterDbStore db, ICharacterCollection census) {

            _Logger = logger;
            _Cache = cache;

            _Db = db;
            _Census = census;
        }

        public async Task<PsCharacter?> GetByID(string charID) {
            string key = string.Format(_CacheKeyID, charID);

            if (_Cache.TryGetValue(key, out PsCharacter? character) == false) {
                character = await _Db.GetByID(charID);

                // Only update the character if it's expired
                if (character == null || await HasExpired(character) == true) {
                    // If we have the character in DB, but not in Census, return it from DB
                    //      Useful if census is down, or a character has been deleted
                    PsCharacter? censusChar = await _Census.GetByID(charID);
                    if (censusChar != null) {
                        character = await _Census.GetByID(charID);
                        await _Db.Upsert(censusChar);
                    }
                }

                _Cache.Set(key, character, new MemoryCacheEntryOptions() {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(20)
                });

                if (character != null) {
                    string nameKey = string.Format(_CacheKeyName, charID);
                    _Cache.Set(nameKey, character?.Name, new MemoryCacheEntryOptions() {
                        SlidingExpiration = TimeSpan.FromHours(2)
                    });
                }
            }

            return character;
        }

        public async Task<PsCharacter?> GetByName(string name) {
            PsCharacter? character;

            string key = string.Format(_CacheKeyName, name);

            if (_Cache.TryGetValue(key, out string charID) == false) {
                character = await _Db.GetByName(name);

                if (character == null || await HasExpired(character) == true) {
                    character = await _Census.GetByName(name);

                    if (character != null) {
                        await _Db.Upsert(character);
                    }
                }

                if (character != null) {
                    _Cache.Set(key, character.ID, new MemoryCacheEntryOptions() {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(20)
                    });

                    string idKey = string.Format(_CacheKeyID, character.ID);
                    _Cache.Set(idKey, character, new MemoryCacheEntryOptions() {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(20)
                    });
                }
            } else {
                character = await GetByID(charID);
            }

            return character;
        }

        public async Task Upsert(PsCharacter character) {
            await _Db.Upsert(character);
            _Cache.Remove(string.Format(_CacheKeyID, character.ID));
            _Cache.Remove(string.Format(_CacheKeyName, character.Name));
        }
        
        private Task<bool> HasExpired(PsCharacter character) {
            return Task.FromResult(character.LastUpdated >= DateTime.UtcNow - TimeSpan.FromHours(24));
        }

    }
}
