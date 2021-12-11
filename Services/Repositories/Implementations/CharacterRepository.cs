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

namespace watchtower.Services.Repositories.Implementations {

    public class CharacterRepository : ICharacterRepository {

        private readonly ILogger<CharacterRepository> _Logger;
        private readonly IMemoryCache _Cache;

        private readonly ICharacterDbStore _Db;
        private readonly ICharacterCollection _Census;

        private readonly BackgroundCharacterWeaponStatQueue _Queue;

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
                ICharacterDbStore db, ICharacterCollection census,
                BackgroundCharacterWeaponStatQueue queue) {

            _Logger = logger;
            _Cache = cache;

            _Db = db;
            _Census = census;

            _Queue = queue;
        }

        public async Task<PsCharacter?> GetByID(string charID) {
            string key = string.Format(CACHE_KEY_ID, charID);

            if (_Cache.TryGetValue(key, out PsCharacter? character) == false) {
                character = await _Db.GetByID(charID);

                // Only update the character if it's expired
                // If the DateLastLogin is the MinValue, it means the column was null from the DB, and it needs to be pulled from census
                if (character == null || await HasExpired(character) == true || character.DateLastLogin == DateTime.MinValue) {
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
                    string nameKey = string.Format(CACHE_KEY_NAME, charID);
                    _Cache.Set(nameKey, character?.Name, new MemoryCacheEntryOptions() {
                        SlidingExpiration = TimeSpan.FromHours(2)
                    });
                }
            }

            return character;
        }

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

        public async Task<List<PsCharacter>> SearchByName(string name) {
            List<PsCharacter> all = new List<PsCharacter>();

            Stopwatch timer = Stopwatch.StartNew();

            List<PsCharacter> db = await _Db.SearchByName(name);
            all.AddRange(db);

            long dbLookup = timer.ElapsedMilliseconds;
            timer.Restart();

            List<PsCharacter> census = new List<PsCharacter>();

            // Setup a task that will be cancelled in the timeout period given, to ensure this method is fast
            Task<List<PsCharacter>> wrapper = Task.Run(() => _Census.SearchByName(name));
            bool censusCancelled = wrapper.Wait(TimeSpan.FromMilliseconds(SEARCH_CENSUS_TIMEOUT_MS)) == false;
            if (censusCancelled == false) {
                census = wrapper.Result;
            } else {
                _Logger.LogWarning($"Census search timed out after {SEARCH_CENSUS_TIMEOUT_MS}ms");
            }

            long censusLookup = timer.ElapsedMilliseconds;
            timer.Restart();

            if (census.Count > 0) {
                new Thread(async () => {
                    Task[] inserts = new Task[census.Count];

                    // Get all characters that exist in census, but don't exist in DB
                    for (int i = 0; i < census.Count; ++i) {
                        PsCharacter c = census[i];
                        PsCharacter? d = db.FirstOrDefault(iter => iter.ID == c.ID);
                        if (d == null) {
                            // Add the task for inserting them into the DB
                            inserts[i] = _Db.Upsert(c);
                            _Queue.Queue(c.ID);
                        } else {
                            // Else they already exist in DB, no need to get
                            inserts[i] = Task.CompletedTask;

                            // If the DateLastLogin is the min value, it means the value isn't set, so lets get it from Census
                            // Usually this would be dumb, cause then you'd have a bunch of deleted characters clogging the queue,
                            //      but since this is an iteration thru a list that comes from Census, the character must exist,
                            //      it's just that that character hasn't been updated in the DB yet
                            if (d.DateLastLogin == DateTime.MinValue) {
                                _Queue.Queue(d.ID);
                            }
                        }
                    }
                    await Task.WhenAll(inserts);
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

        public async Task Upsert(PsCharacter character) {
            await _Db.Upsert(character);
            _Cache.Remove(string.Format(CACHE_KEY_ID, character.ID));
            _Cache.Remove(string.Format(CACHE_KEY_NAME, character.Name));
        }
        
        private Task<bool> HasExpired(PsCharacter character) {
            return Task.FromResult(character.LastUpdated < DateTime.UtcNow + TimeSpan.FromHours(24));
        }

    }
}
