using DaybreakGames.Census.Exceptions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Models.Census;
using watchtower.Services.Census;
using watchtower.Services.Db;
using watchtower.Services.Queues;

namespace watchtower.Services.Repositories {

    /// <summary>
    ///     Repository to get characters
    /// </summary>
    public class CharacterRepository {

        private readonly ILogger<CharacterRepository> _Logger;
        private readonly IMemoryCache _Cache;

        private readonly CharacterDbStore _Db;
        private readonly CharacterCollection _Census;

        private readonly CharacterUpdateQueue _Queue;
        private readonly CharacterCacheQueue _CacheQueue;

        private const string CACHE_KEY_ID = "Character.ID.{0}"; // {0} => char ID
        private const string CACHE_KEY_NAME = "Character.Name.{0}"; // {0} => char ID

        /// <summary>
        ///     How many milliseconds to wait when searching by Census. Once this time has elapsed, the search
        ///     is cancelled, and only DB results are used
        /// </summary>
        /// <remarks>
        ///     With the couple of tests I've done, the quick response takes around 600ms when loading ~30 entries from Census
        /// </remarks>
        private const int SEARCH_CENSUS_TIMEOUT_MS = 600;

        public CharacterRepository(ILogger<CharacterRepository> logger, IMemoryCache cache,
                CharacterDbStore db, CharacterCollection census,
                CharacterUpdateQueue queue, CharacterCacheQueue cacheQueue) {

            _Logger = logger;
            _Cache = cache;

            _Db = db;
            _Census = census;

            _Queue = queue;
            _CacheQueue = cacheQueue;
        }

        /// <summary>
        ///     Get a single character by ID. If the load failed from Census for some reason, a fallback of what's in the DB
        ///     is used if possible
        /// </summary>
        /// <param name="charID">ID of the character</param>
        public async Task<PsCharacter?> GetByID(string charID) {
            string key = string.Format(CACHE_KEY_ID, charID);

            // When Honu fails to get a character cause of a timeout, don't cache a potentially outdated
            //      character and wait a bit until getting from the cache again
            bool doShortCache = false;

            if (_Cache.TryGetValue(key, out PsCharacter? character) == false) {
                character = await _Db.GetByID(charID);

                // Only update the character if it's expired
                // If the DateLastLogin is the MinValue, it means the column was null from the DB, and it needs to be pulled from census
                if (character == null || HasExpired(character) == true || character.DateLastLogin == DateTime.MinValue) {
                    try {
                        // If we have the character in DB, but not in Census, return it from DB
                        //      Useful if census is down, or a character has been deleted
                        PsCharacter? censusChar = await _Census.GetByID(charID);
                        if (censusChar != null) {
                            character = censusChar;
                            await _Db.Upsert(censusChar);
                        }
                    } catch (Exception ex) {
                        // If Honu failed to find any character, propogate the error up
                        //      else, since we have the character, but it's out of date, use that one instead
                        if (character == null || ex is not CensusConnectionException) {
                            throw;
                        } else {
                            doShortCache = true;
                            _Logger.LogWarning($"timeout when getting {charID}");
                        }
                    }
                }

                _Cache.Set(key, character, new MemoryCacheEntryOptions() {
                    AbsoluteExpirationRelativeToNow = doShortCache == true ? TimeSpan.FromMinutes(1) : TimeSpan.FromMinutes(20)
                });

                if (character != null) {
                    string nameKey = string.Format(CACHE_KEY_NAME, charID);
                    _Cache.Set(nameKey, character?.Name, new MemoryCacheEntryOptions() {
                        SlidingExpiration = TimeSpan.FromHours(2)
                    });
                }
            }

            return character;
        }

        /// <summary>
        ///     Get all the characters that have an ID
        /// </summary>
        /// <param name="IDs">List of IDs to load</param>
        /// <param name="fast">If only characters from DB will be loaded, and any characters not in the DB will be queued for retrieval</param>
        /// <returns></returns>
        public async Task<List<PsCharacter>> GetByIDs(List<string> IDs, bool fast = false) {
            List<PsCharacter> chars = new List<PsCharacter>(IDs.Count);

            int total = IDs.Count;
            int found = 0;

            //_Logger.LogTrace($"Loading {total} characters");

            Stopwatch timer = Stopwatch.StartNew();
            int inCache = 0;
            foreach (string ID in IDs.ToList()) {
                string cacheKey = string.Format(CACHE_KEY_ID, ID);

                if (_Cache.TryGetValue(cacheKey, out PsCharacter? character) == true) {
                    if (character != null) {
                        chars.Add(character);
                        IDs.Remove(ID);
                        ++inCache;
                        ++found;
                    }
                }
            }
            long toCache = timer.ElapsedMilliseconds;
            timer.Restart();

            //_Logger.LogTrace($"Took {toCache}ms to load from cache");

            long toDb = 0;
            int inDb = 0;
            int inExpired = 0;
            List<PsCharacter> db = new List<PsCharacter>();
            if (found < total) {
                db = await _Db.GetByIDs(IDs);

                foreach (PsCharacter c in db) {
                    if (fast == true) {
                        IDs.Remove(c.ID);
                        chars.Add(c);
                        ++inDb;
                        ++found;
                    } else {
                        if (HasExpired(c) == false) {
                            IDs.Remove(c.ID);
                            chars.Add(c);
                            ++inDb;
                            ++found;
                        } else {
                            _Queue.Queue(c.ID);
                            _CacheQueue.Queue(c.ID);
                            ++inExpired;
                        }
                    }
                }
                toDb = timer.ElapsedMilliseconds;
                timer.Restart();

                //_Logger.LogTrace($"Took {toDb}ms to load from db");
            }

            long toCensus = 0;
            int inCensus = 0;
            if (found < total) {
                if (fast == false) {
                    List<PsCharacter> census = await _Census.GetByIDs(IDs);

                    foreach (PsCharacter c in census) {
                        IDs.Remove(c.ID);
                        chars.Add(c);
                        ++inCensus;
                        ++found;
                    }
                } else {
                    foreach (string ID in IDs) {
                        _CacheQueue.Queue(ID);
                    }
                }
                toCensus = timer.ElapsedMilliseconds;
                //_Logger.LogTrace($"Took {toCensus}ms to load from census");
            }

            // If a character in the DB was ignored because it had expired, but Honu still doesn't have the character at this point, load it anyways
            int inDbRescue = 0;
            if (found < total) {
                foreach (string id in IDs.ToList()) {
                    PsCharacter? c = db.FirstOrDefault(iter => iter.ID == id);
                    if (c != null) {
                        ++inDbRescue;
                        chars.Add(c);
                        IDs.Remove(c.ID);
                        ++found;
                    }
                }
            }

            foreach (PsCharacter c in chars) {
                string cacheKey = string.Format(CACHE_KEY_ID, c.ID);
                _Cache.Set(cacheKey, c, new MemoryCacheEntryOptions() {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(20)
                });
            }

            /*
            _Logger.LogDebug($"Found {chars.Count}/{total} characters. "
                + $"In cache: {inCache} in {toCache}ms, db: {inDb} ({inExpired} expired) in {toDb}ms, census: {inCensus} in {toCensus}ms, rescue: {inDbRescue}, left: {IDs.Count}");
            */

            return chars;
        }

        /// <summary>
        ///     Get all characters that match the name (case-insensitive)
        /// </summary>
        /// <remarks>
        ///     This return is a list, as deleted characters name have may be reused
        /// </remarks>
        /// <param name="name">Name to get</param>
        /// <returns></returns>
        public async Task<List<PsCharacter>> GetByName(string name) {
            string key = string.Format(CACHE_KEY_NAME, name);

            if (_Cache.TryGetValue(key, out List<PsCharacter> characters) == false) {
                characters = await _Db.GetByName(name);

                PsCharacter? live = await _Census.GetByName(name);

                // If the character exists in db and census only add once
                if (live != null && characters.FirstOrDefault(iter => iter.ID == live.ID) == null) {
                    characters.Add(live);
                }

                _Cache.Set(key, characters, new MemoryCacheEntryOptions() {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(20)
                });
            }

            return characters;
        }

        /// <summary>
        ///     Search for characters that have a partial match to a name
        /// </summary>
        /// <param name="name">Name to search</param>
        /// <param name="timeoutCensus">If the census request will be timed out or not</param>
        /// <returns></returns>
        public async Task<List<PsCharacter>> SearchByName(string name, bool timeoutCensus = true) {
            List<PsCharacter> all = new List<PsCharacter>();

            Stopwatch timer = Stopwatch.StartNew();

            List<PsCharacter> db = await _Db.SearchByName(name);
            all.AddRange(db);

            long dbLookup = timer.ElapsedMilliseconds;
            timer.Restart();

            List<PsCharacter> census = new List<PsCharacter>();

            // Setup a task that will be cancelled in the timeout period given, to ensure this method is fast
            Task<List<PsCharacter>> wrapper = Task.Run(() => _Census.SearchByName(name, CancellationToken.None));
            bool censusCancelled = wrapper.Wait((timeoutCensus == true) ? TimeSpan.FromMilliseconds(SEARCH_CENSUS_TIMEOUT_MS) : TimeSpan.FromSeconds(60)) == false;
            if (censusCancelled == false) {
                census = wrapper.Result;
            } else {
                //_Logger.LogWarning($"Census search timed out after {SEARCH_CENSUS_TIMEOUT_MS}ms");
            }

            long censusLookup = timer.ElapsedMilliseconds;
            timer.Restart();

            if (census.Count > 0) {
                new Thread(async () => {
                    try {
                        // Get all characters that exist in census, but don't exist in DB
                        for (int i = 0; i < census.Count; ++i) {
                            PsCharacter c = census[i];
                            PsCharacter? d = db.FirstOrDefault(iter => iter.ID == c.ID);
                            if (d == null) {
                                await _Db.Upsert(c);
                                _Queue.Queue(c.ID);
                            } else {
                                // If the DateLastLogin is the min value, it means the value isn't set, so lets get it from Census
                                // Usually this would be dumb, cause then you'd have a bunch of deleted characters clogging the queue,
                                //      but since this is an iteration thru a list that comes from Census, the character must exist,
                                //      it's just that that character hasn't been updated in the DB yet
                                if (d.DateLastLogin == DateTime.MinValue) {
                                    _Queue.Queue(d.ID);
                                }
                            }
                        }
                    } catch (Exception ex) {
                        _Logger.LogError(ex, $"Error while performing update on {census.Count} characters");
                    }
                }).Start();
            }

            all = all.Distinct()
                .OrderByDescending(iter => iter.DateLastLogin).ToList();

            long sortTime = timer.ElapsedMilliseconds;

            _Logger.LogDebug($"Timings to lookup '{name}':\n"
                + $"\tDB search: {dbLookup}ms\n"
                + $"\tCensus search: {censusLookup}ms {(censusCancelled ? "(cancelled)" : "")}\n"
                + $"\tSort: {sortTime}ms"
            );

            return all;
        }
        
        private bool HasExpired(PsCharacter character) {
            return character.LastUpdated < DateTime.UtcNow + TimeSpan.FromHours(24);
        }

    }

    public static class ICharacterRepositoryExtensions {

        /// <summary>
        ///     Get a single character by name, with the character with the most recent logged on character being used
        ///     if multiple characters with the same name exist
        /// </summary>
        /// <param name="repo"></param>
        /// <param name="name">Name to get (case-insensitive)</param>
        /// <returns></returns>
        public static async Task<PsCharacter?> GetFirstByName(this CharacterRepository repo, string name) {
            List<PsCharacter> chars = await repo.GetByName(name);
            chars = chars.OrderByDescending(iter => iter.DateLastLogin).ToList();

            if (chars.Count == 0) {
                return null;
            }

            return chars[0];
        }

    }

}
