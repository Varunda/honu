
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

    public class CharacterFriendRepository {

        private readonly ILogger<CharacterFriendRepository> _Logger;
        private readonly IMemoryCache _Cache;

        private readonly CharacterFriendCollection _Census;
        private readonly CharacterFriendDbStore _Db;

        private readonly CharacterMetadataDbStore _MetadataDb;

        private const string CACHE_KEY = "CharacterFriends.{0}"; // {0} => Character ID

        public CharacterFriendRepository(ILogger<CharacterFriendRepository> logger,
            CharacterFriendCollection census, CharacterFriendDbStore db,
            IMemoryCache cache, CharacterMetadataDbStore metadataDb) {

            _Logger = logger;
            _Cache = cache;

            _Census = census ?? throw new ArgumentNullException(nameof(census));
            _Db = db ?? throw new ArgumentNullException(nameof(db));
            _MetadataDb = metadataDb;
        }

        /// <summary>
        ///     Get the friends of a character by using both Census and the local DB
        /// </summary>
        /// <param name="charID">ID of the character</param>
        public async Task<List<CharacterFriend>> GetByCharacterID(string charID) {
            string cacheKey = string.Format(CACHE_KEY, charID);
            if (_Cache.TryGetValue(cacheKey, out List<CharacterFriend> friends) == false) {
                friends = await _Db.GetByCharacterID(charID);

                _Logger.LogTrace($"Got {friends.Count} entries from DB");

                // Get the friends from Census if they have no friends, they have no metadata, or the last
                //      time honu updated them was more than a day ago
                //
                // TODO: Get the last login and compare to metadata???
                bool fetchCensus = friends.Count == 0;
                if (fetchCensus == false) {
                    CharacterMetadata? metadata = await _MetadataDb.GetByCharacterID(charID);
                    fetchCensus = (metadata == null) || (DateTime.UtcNow - metadata.LastUpdated > TimeSpan.FromDays(1));
                }

                if (fetchCensus == true) {
                    _Logger.LogTrace($"DB is invalid for some reason, getting from Census");
                    List<CharacterFriend> censusFriends = await _Census.GetByCharacterID(charID);
                    // Only set if they have friends, else you risk deleting data from deleted characters
                    if (censusFriends.Count > 0) {
                        friends = censusFriends;
                        await _Db.Set(charID, censusFriends);
                    }
                }

                _Cache.Set(cacheKey, friends, new MemoryCacheEntryOptions() {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
                });
            }

            return friends;
        }

    }

}