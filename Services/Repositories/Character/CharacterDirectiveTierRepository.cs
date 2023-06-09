using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using watchtower.Models.Census;
using watchtower.Models.Db;
using watchtower.Services.Census;
using watchtower.Services.Db;

namespace watchtower.Services.Repositories {

    public class CharacterDirectiveTierRepository {

        private readonly ILogger<CharacterDirectiveTierRepository> _Logger;
        private readonly IMemoryCache _Cache;

        private readonly CharacterMetadataDbStore _Metadata;
        private readonly CharacterDbStore _CharacterDb;
        private readonly CharacterDirectiveTierCollection _Census;
        private readonly CharacterDirectiveTierDbStore _Db;

        private const string CACHE_KEY = "CharacterDirectiveTier.{0}"; // {0} => Character ID

        public CharacterDirectiveTierRepository(ILogger<CharacterDirectiveTierRepository> logger,
            IMemoryCache cache, CharacterDirectiveTierCollection census,
            CharacterDirectiveTierDbStore db, CharacterMetadataDbStore metadata,
            CharacterDbStore charDb) {

            _Logger = logger;
            _Cache = cache;

            _Metadata = metadata;
            _CharacterDb = charDb;
            _Census = census;
            _Db = db;
        }

        public async Task<List<CharacterDirectiveTier>> GetByCharacterID(string charID) {
            string cacheKey = string.Format(CACHE_KEY, charID);

            if (_Cache.TryGetValue(cacheKey, out List<CharacterDirectiveTier> dirs) == false) {
                CharacterMetadata? metadata = await _Metadata.GetByCharacterID(charID);
                PsCharacter? dbChar = await _CharacterDb.GetByID(charID);

                // Metadata doesn't exist, character doesn't exist in db, or it hasn't been updated
                bool getCensus = metadata == null || dbChar == null || (metadata.LastUpdated < dbChar.DateLastLogin);

                // If we're gonna go to Census anyways, can just skip this bit, saving a db call
                if (getCensus == false) {
                    dirs = await _Db.GetByCharacterID(charID);
                    getCensus = dirs.Count == 0; // If there is no character directives, might not have gotten yet
                }

                if (getCensus == true) {
                    dirs = await _Census.GetByCharacterID(charID);

                    foreach (CharacterDirectiveTier dir in dirs) {
                        await _Db.Upsert(charID, dir);
                    }
                }

                _Cache.Set(cacheKey, dirs, new MemoryCacheEntryOptions() {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(20)
                });
            }

            return dirs;
        }

        public async Task Upsert(string charID, CharacterDirectiveTier tier) {
            string cacheKey = string.Format(CACHE_KEY, charID);
            _Cache.Remove(cacheKey);

            await _Db.Upsert(charID, tier);
        }

    }

}