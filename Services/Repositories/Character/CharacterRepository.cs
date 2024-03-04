using DaybreakGames.Census.Exceptions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Code.Constants;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Census;
using watchtower.Models.Db;
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
        private readonly CharacterNameChangeDbStore _CharacterNameChangeDb;

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

        private int _CircuitBreakerErrors = 0;
        private DateTime? _CircuitBreakerTimeout = null;
        private DateTime _CircuitBreakerInformed = DateTime.UtcNow;

        private bool _IsCircuitBreakerOn {
            get {
                return _CircuitBreakerTimeout != null && DateTime.UtcNow < _CircuitBreakerTimeout;
            }
        }

        public CharacterRepository(ILogger<CharacterRepository> logger, IMemoryCache cache,
            CharacterDbStore db, CharacterCollection census,
            CharacterUpdateQueue queue, CharacterCacheQueue cacheQueue,
            CharacterNameChangeDbStore characterNameChangeDb) {

            _Logger = logger;
            _Cache = cache;

            _Db = db;
            _Census = census;

            _Queue = queue;
            _CacheQueue = cacheQueue;
            _CharacterNameChangeDb = characterNameChangeDb;
        }

        /// <summary>
        ///     Get a single character by ID. If the load failed from Census for some reason, a fallback of what's in the DB
        ///     is used if possible
        /// </summary>
        /// <param name="charID">ID of the character</param>
        /// <param name="env">Which census environment, such as PC, PS4 EU or PS4 US</param>
        /// <param name="fast">
        ///     If the character is not found in the DB, will a Census call be made?
        ///     If false and not found in the DB, the character will be put into a queue to be pulled later
        /// </param>
        /// <returns>
        ///     The <see cref="PsCharacter"/> with <see cref="PsCharacter.ID"/> of <paramref name="charID"/>,
        ///     or <c>null</c> if it could not be found in the available data sources. If <paramref name="fast"/>
        ///     is <c>true</c>, it is possible that it exists in Census, but not locally
        /// </returns>
        public async Task<PsCharacter?> GetByID(string charID, CensusEnvironment env, bool fast = false) {
            string key = string.Format(CACHE_KEY_ID, charID);

            // When Honu fails to get a character cause of a timeout, don't cache a potentially outdated
            //      character and wait a bit until getting from the cache again
            bool doShortCache = false;

            if (_Cache.TryGetValue(key, out PsCharacter? character) == false) {
                character = await _Db.GetByID(charID);

                // Only update the character if it's expired
                // If the DateLastLogin is the MinValue, it means the column was null from the DB, and it needs to be pulled from census
                // If fast is true, only use the DB to get the data
                if (fast == false && (character == null || HasExpired(character) == true || character.DateLastLogin == DateTime.MinValue)) {
                    try {

                        // check the circuit breaker
                        PsCharacter? censusChar;
                        if (_IsCircuitBreakerOn) {
                            censusChar = null;
                            doShortCache = true;
                            if (_CircuitBreakerInformed <= DateTime.UtcNow) {
                                _CircuitBreakerInformed = DateTime.UtcNow + TimeSpan.FromSeconds(15);
                                _Logger.LogWarning($"circuit breaker is on, bypassing Census call [nexted informated={_CircuitBreakerInformed:u}] [bypassed=get by id]");
                            }
                        } else {
                            censusChar = await (_Census.GetByID(charID, env).TimeoutWithDefault(TimeSpan.FromSeconds(10), null));
                        }

                        // If we have the character in DB, but not in Census, return it from DB
                        //      Useful if census is down, or a character has been deleted
                        if (censusChar != null) {

                            // if there is a character from the db, and a character from Census, lets compare the names
                            if (character != null) {
                                //_Logger.LogDebug($"comparing {character.Name} to {censusChar.Name}");
                                if (character.Name != censusChar.Name) {
                                    _Logger.LogInformation($"name change detected! [character ID={character.ID}] [old name={censusChar.Name}] [new name={character.Name}]");

                                    CharacterNameChange change = new();
                                    change.CharacterID = charID;
                                    change.OldName = censusChar.Name;
                                    change.NewName = character.Name;
                                    change.LowerBound = censusChar.LastUpdated;
                                    change.UpperBound = character.LastUpdated;
                                    change.Timestamp = DateTime.UtcNow;

                                    try {
                                        await _CharacterNameChangeDb.Insert(change);
                                    } catch (Exception ex) {
                                        _Logger.LogError(ex, $"failed to insert name change [character ID={character.ID}]!");
                                    }
                                }
                            }

                            // if we got a character from Census fine, turn the circuit breaker off
                            if (_CircuitBreakerErrors > 0) {
                                _Logger.LogInformation($"circuit breaker passed, resetting count");
                            }
                            _CircuitBreakerTimeout = null;
                            _CircuitBreakerErrors = 0;

                            character = censusChar;
                            await _Db.Upsert(censusChar);
                        }
                    } catch (Exception ex) {
                        ++_CircuitBreakerErrors;
                        if (_CircuitBreakerErrors >= 10) {
                            _CircuitBreakerTimeout = DateTime.UtcNow + TimeSpan.FromSeconds(30);
                            _Logger.LogError($"tripping circuit breaker! [_CircuitBreakerErrors={_CircuitBreakerErrors}] [_CircuitBreakerTimeout={_CircuitBreakerTimeout:u}]");
                        }

                        // If Honu failed to find any character, propogate the error up
                        //      else, since we have the character, but it's out of date, use that one instead
                        if (character == null) {  // || ex is not CensusConnectionException) {
                            _Logger.LogError($"failed to get character from Census [charID={charID}] [_CircuitBreakerErrors={_CircuitBreakerErrors}] [Exception={ex.Message}]");
                            throw;
                        } else {
                            doShortCache = true;
                            _Logger.LogWarning($"rescued character from DB due to failed Census call [charID={charID}] [_CircuitBreakerErrors={_CircuitBreakerErrors}] [Exception={ex.Message}]");
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
        /// <param name="env">Which census environment, such as PC, PS4 EU or PS4 US</param>
        /// <param name="fast">If only characters from DB will be loaded, and any characters not in the DB will be queued for retrieval</param>
        /// <returns></returns>
        public async Task<List<PsCharacter>> GetByIDs(IEnumerable<string> IDs, CensusEnvironment env, bool fast = false) {
            if (!IDs.Any()) {
                return new List<PsCharacter>();
            }

            const int BLOCK_SIZE = 2000;

            if (IDs.Count() <= BLOCK_SIZE) {
                return await GetByIDsInternal(IDs, env, fast);
            }

            int BLOCK_COUNT = (int)Math.Ceiling((double) IDs.Count() / BLOCK_SIZE);

            _Logger.LogDebug($"Encounted large character request size of {IDs.Count()}, breaking into {BLOCK_COUNT} blocks of {BLOCK_SIZE}");

            List<PsCharacter> chars = new List<PsCharacter>(IDs.Count());
            for (int i = 0; i < BLOCK_COUNT; ++i) {
                List<string> block = IDs.Skip(i * BLOCK_SIZE).Take(BLOCK_SIZE).ToList();
                if (block.Count == 0) {
                    continue;
                }

                chars.AddRange(await GetByIDsInternal(block, env, fast));
            }

            return chars;
        }

        private async Task<List<PsCharacter>> GetByIDsInternal(IEnumerable<string> IDs, CensusEnvironment env, bool fast = false) {
            if (!IDs.Any()) {
                return new List<PsCharacter>();
            }

            // Make a copy of the IDs as this list gets modified, and if the passed list is modified,
            //      that could affect whatever is calling this method
            List<string> localIDs = new List<string>(IDs);
            List<PsCharacter> chars = new List<PsCharacter>(localIDs.Count);

            int total = IDs.Count();
            int found = 0;

            //_Logger.LogTrace($"Loading {total} characters");

            Stopwatch timer = Stopwatch.StartNew();
            int inCache = 0;
            foreach (string ID in localIDs.ToList()) {
                string cacheKey = string.Format(CACHE_KEY_ID, ID);

                if (_Cache.TryGetValue(cacheKey, out PsCharacter? character) == true) {
                    if (character != null) {
                        chars.Add(character);
                        localIDs.Remove(ID);
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
                db = await _Db.GetByIDs(localIDs);

                foreach (PsCharacter c in db) {
                    if (fast == true) {
                        localIDs.Remove(c.ID);
                        chars.Add(c);
                        ++inDb;
                        ++found;
                    } else {
                        if (HasExpired(c) == false) {
                            localIDs.Remove(c.ID);
                            chars.Add(c);
                            ++inDb;
                            ++found;
                        } else {
                            _Queue.Queue(c.ID);

                            _CacheQueue.Queue(c.ID, env);

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
                    List<PsCharacter> census;
                    try {
                        if (_IsCircuitBreakerOn) {
                            census = new List<PsCharacter>();
                            if (_CircuitBreakerInformed <= DateTime.UtcNow) {
                                _CircuitBreakerInformed = DateTime.UtcNow + TimeSpan.FromSeconds(15);
                                _Logger.LogWarning($"circuit breaker is on, bypassing Census call [nexted informated={_CircuitBreakerInformed:u}] [bypassed=get by ids]");
                            }
                        } else {
                            census = await _Census.GetByIDs(localIDs, env);
                        }

                        // if we got a character from Census fine, turn the circuit breaker off
                        if (_CircuitBreakerErrors > 0) {
                            _Logger.LogInformation($"circuit breaker passed, resetting count");
                        }
                        _CircuitBreakerTimeout = null;
                        _CircuitBreakerErrors = 0;
                    } catch (Exception ex) {
                        ++_CircuitBreakerErrors;
                        if (_CircuitBreakerErrors >= 10) {
                            _CircuitBreakerTimeout = DateTime.UtcNow + TimeSpan.FromSeconds(30);
                            _Logger.LogError($"tripping circuit breaker! [_CircuitBreakerErrors={_CircuitBreakerErrors}] [_CircuitBreakerTimeout={_CircuitBreakerTimeout:u}]");
                        }

                        _Logger.LogError(ex, $"failed to characters from Census");
                        census = new List<PsCharacter>();
                    }

                    foreach (PsCharacter c in census) {
                        localIDs.Remove(c.ID);
                        chars.Add(c);
                        ++inCensus;
                        ++found;
                    }
                } else {
                    foreach (string ID in localIDs) {
                        _CacheQueue.Queue(ID, env);
                    }
                }
                toCensus = timer.ElapsedMilliseconds;
                //_Logger.LogTrace($"Took {toCensus}ms to load from census");
            }

            // If a character in the DB was ignored because it had expired, but Honu still doesn't have the character at this point, load it anyways
            int inDbRescue = 0;
            if (found < total) {
                foreach (string id in localIDs.ToList()) {
                    PsCharacter? c = db.FirstOrDefault(iter => iter.ID == id);
                    if (c != null) {
                        ++inDbRescue;
                        chars.Add(c);
                        localIDs.Remove(c.ID);
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
            if (toDb > 100 || toCensus > 100) {
                _Logger.LogDebug($"Found {chars.Count}/{total} characters. "
                    + $"In cache: {inCache} in {toCache}ms, db: {inDb} ({inExpired} expired) in {toDb}ms, census: {inCensus} in {toCensus}ms, rescue: {inDbRescue}, left: {localIDs.Count}");
            }
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
        /// <param name="fast">If true, no Census lookup is done, only a DB search</param>
        /// <returns></returns>
        public async Task<List<PsCharacter>> GetByName(string name, bool fast = false) {
            string key = string.Format(CACHE_KEY_NAME, name);

            if (_Cache.TryGetValue(key, out List<PsCharacter> characters) == false) {
                characters = await _Db.GetByName(name);

                PsCharacter? live = (fast == false) ? await _Census.GetByName(name) : null;

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
        ///     Search for characters that have a partial match to a name, currently only supports PC
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

            _Logger.LogDebug($"Timings to lookup '{name}': "
                + $"[DB search]{dbLookup}ms] "
                + $"[Census search]{censusLookup}ms {(censusCancelled ? "(cancelled)" : "")}] "
                + $"[Sort={sortTime}ms]"
            );

            return all;
        }
        
        private bool HasExpired(PsCharacter character) {
            return character.LastUpdated < DateTime.UtcNow + TimeSpan.FromHours(24);
        }

        /// <summary>
        ///     Upsert (update/insert) a character into the repository. Takes care of removing cached data
        /// </summary>
        /// <param name="c">Character to upsert</param>
        /// <returns>
        ///     A task when the operation is complete
        /// </returns>
        public async Task Upsert(PsCharacter c) {
            string key = string.Format(CACHE_KEY_ID, c.ID);
            _Cache.Remove(key);

            await _Db.Upsert(c);
        }

    }

    public static class ICharacterRepositoryExtensions {

        /// <summary>
        ///     Get a single character by name, with the character with the most recent logged on character being used
        ///     if multiple characters with the same name exist
        /// </summary>
        /// <param name="repo"></param>
        /// <param name="name">Name to get (case-insensitive)</param>
        /// <param name="fast">If true, no Census query will be done</param>
        /// <returns></returns>
        public static async Task<PsCharacter?> GetFirstByName(this CharacterRepository repo, string name, bool fast = false) {
            List<PsCharacter> chars = await repo.GetByName(name, fast);
            chars = chars.OrderByDescending(iter => iter.DateLastLogin).ToList();

            if (chars.Count == 0) {
                return null;
            }

            return chars[0];
        }

    }

}
