using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using watchtower.Models.Census;
using watchtower.Models.Db;
using watchtower.Services.Census;
using watchtower.Services.Db;

namespace watchtower.Services.Repositories {

    public class CharacterAchievementRepository {

        private readonly ILogger<CharacterAchievementRepository> _Logger;
        private readonly IMemoryCache _Cache;

        private const string CACHE_KEY = "CharacterAchievement.{0}"; // {0} => Character ID

        private readonly CharacterAchievementCollection _Census;
        private readonly CharacterAchievementDbStore _Db;

        private readonly CharacterMetadataDbStore _MetadataDb;

        public CharacterAchievementRepository(ILogger<CharacterAchievementRepository> logger,
            CharacterAchievementCollection charAchCensus, CharacterAchievementDbStore charAchDb,
            IMemoryCache cache, CharacterMetadataDbStore metadataDb) {

            _Logger = logger;
            _Cache = cache;

            _Census = charAchCensus;
            _Db = charAchDb;

            _MetadataDb = metadataDb;
        }

        public async Task<List<CharacterAchievement>> GetByCharacterID(string charID) {
            string cacheKey = string.Format(CACHE_KEY, charID);

            if (_Cache.TryGetValue(cacheKey, out List<CharacterAchievement> achs) == false) {
                achs = await _Db.GetByCharacterID(charID);

                // Get the friends from Census if they have no friends, they have no metadata, or the last
                //      time honu updated them was more than a day ago
                //
                // TODO: Get the last login and compare to metadata???
                bool fetchCensus = achs.Count == 0;
                if (fetchCensus == false) {
                    CharacterMetadata? metadata = await _MetadataDb.GetByCharacterID(charID);
                    fetchCensus = (metadata == null) || (DateTime.UtcNow - metadata.LastUpdated > TimeSpan.FromDays(1));
                }

                if (fetchCensus == true) {
                    List<CharacterAchievement> censusAchs = await _Census.GetByCharacterID(charID);
                    if (censusAchs.Count > 0) {
                        achs = censusAchs;

                        foreach (CharacterAchievement charAch in achs) {
                            await _Db.Upsert(charAch);
                        }
                    }
                }

                _Cache.Set(cacheKey, achs, new MemoryCacheEntryOptions() {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
                });
            }

            return achs;
        }


    }
}
