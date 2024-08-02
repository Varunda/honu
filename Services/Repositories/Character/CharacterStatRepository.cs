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

            if (_Cache.TryGetValue(cacheKey, out List<PsCharacterStat>? stats) == false || stats == null) {
                stats = await _Db.GetByID(charID);

                bool getCensus = (stats.Count == 0);
                if (getCensus == false) {
                    DateTime max = stats.Max(iter => iter.Timestamp);

                    if (DateTime.UtcNow - max > TimeSpan.FromDays(1)) {
                        getCensus = true;
                    }
                }

                try {
                    if (getCensus == true) {
                        Task<List<PsCharacterStat>> censusStats = Task.Run(() => _Census.GetByID(charID));
                        // if honu has history stats for the character from the db (but are possibly out of date),
                        //      then use a quicker timeout to be more responsive
                        TimeSpan timeout = stats.Count > 0 ? TimeSpan.FromSeconds(10) : TimeSpan.FromSeconds(60);
                        bool cancelled = censusStats.Wait(timeout) == false;
                        if (cancelled == false) {
                            List<PsCharacterStat> census = censusStats.Result;

                            if (census.Count > 0) {
                                await _Db.Set(charID, census);
                            }

                            stats = census;
                        } else {
                            // if honu has no DB stats, and failed the census call, throw an exception cause the repo failed to load them
                            if (stats.Count == 0) {
                                throw new Exception($"failed to load character stats for {charID}, DB had no stats, and Census timed out");
                            } else {
                                _Logger.LogDebug($"cancelled request for census stats due to timeout of {timeout}");
                            }
                        }
                    }
                } catch (Exception ex) {
                    if (stats.Count > 0) {
                        _Logger.LogWarning($"failed to get character stats (saved from db) [charID={charID}] [exception={ex.Message}]");
                    } else {
                        _Logger.LogError($"failed to get character stats (not saved in DB) [charID={charID}]");
                        throw;
                    }
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
