using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using watchtower.Services.Db;

namespace watchtower.Services.Repositories {

    public class HonuMetadataRepository {

        private readonly ILogger<HonuMetadataRepository> _Logger;
        private readonly HonuMetadataDbStore _Db;
        private readonly IMemoryCache _Cache;

        private const string CACHE_KEY = "Honu.Metadata.{0}"; // {0} => key

        public HonuMetadataRepository(ILogger<HonuMetadataRepository> logger,
            HonuMetadataDbStore db, IMemoryCache cache) {

            _Logger = logger;
            _Db = db;
            _Cache = cache;
        }

        /// <summary>
        ///     Get a metadata value
        /// </summary>
        /// <param name="key">case-insensitive key to get</param>
        /// <returns>
        ///     Null if no value is set, otherwise the string value
        /// </returns>
        public async Task<string?> Get(string key) {
            key = key.ToLower();

            string cacheKey = string.Format(CACHE_KEY, key);

            if (_Cache.TryGetValue(cacheKey, out string? str) == false) {
                str = await _Db.Get(key);

                _Cache.Set(cacheKey, str, new MemoryCacheEntryOptions() {
                    SlidingExpiration = TimeSpan.FromMinutes(10)
                });
            }

            return str;
        }

        /// <summary>
        ///     Update/Insert (upsert) a metadata value
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public async Task Upsert(string key, string value) {
            string cacheKey = string.Format(CACHE_KEY, key);
            _Cache.Remove(cacheKey);

            await _Db.Upsert(key, value);
        }

    }

    public static class HonuMetadataRepositoryExtensions {

        /// <summary>
        ///     Get a value as a string. Keys are case-insensitive!
        /// </summary>
        /// <param name="repo">extension instance</param>
        /// <param name="key">key of the obj</param>
        /// <returns>
        ///     A nullable boolean value based on the string value read from the DB
        /// </returns>
        /// <exception cref="FormatException">
        ///     If the value read from the DB was not "true" or "false", the only valid values (or null)
        /// </exception>
        public static async Task<bool?> GetAsBoolean(this HonuMetadataRepository repo, string key) {
            string? v = await repo.Get(key);

            if (v == null) {
                return null;
            }

            if (v == "true") {
                return true;
            } else if (v == "false") {
                return false;
            }

            throw new FormatException($"failed to convert '{v}' to true/false");
        }

        /// <summary>
        ///     Upsert a boolean
        /// </summary>
        /// <param name="repo"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static async Task Upsert(this HonuMetadataRepository repo, string key, bool value) {

            await repo.Upsert(key, value ? "true" : "false");
        }

    }
}
