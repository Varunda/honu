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

namespace watchtower.Services.Repositories {

    public class OutfitRepository {

        private readonly ILogger<OutfitRepository> _Logger;
        private readonly IMemoryCache _Cache;

        private const string _CacheKeyID = "Outfit.ID.{0}"; // {0} => Outfit ID
        private const string CACHE_KEY_MEMBERS = "Outfit.Members.{0}"; // {0} => Outfit ID

        private readonly OutfitDbStore _Db;
        private readonly OutfitCollection _Census;

        /// <summary>
        ///     How many milliseconds to wait when searching by Census. Once this time has elapsed, the search
        ///     is cancelled, and only DB results are used
        /// </summary>
        /// <remarks>
        ///     With the couple of tests I've done, the quick response takes around 600ms when loading ~30 entries from Census
        /// </remarks>
        private const int SEARCH_CENSUS_TIMEOUT_MS = 600;

        public OutfitRepository(ILogger<OutfitRepository> logger, IMemoryCache cache,
            OutfitDbStore db, OutfitCollection coll) {

            _Logger = logger;
            _Cache = cache;

            _Db = db ?? throw new ArgumentNullException(nameof(db));
            _Census = coll ?? throw new ArgumentNullException(nameof(coll));
        }

        /// <summary>
        ///     Get an <see cref="PsOutfit"/> by ID
        /// </summary>
        /// <param name="outfitID">ID of the outfit to get</param>
        /// <returns>
        ///     The <see cref="PsOutfit"/> with <see cref="PsOutfit.ID"/> of <paramref name="outfitID"/>
        /// </returns>
        public async Task<PsOutfit?> GetByID(string outfitID) {
            if (outfitID == "") {
                return null;
            }

            string key = string.Format(_CacheKeyID, outfitID);

            if (_Cache.TryGetValue(key, out PsOutfit? outfit) == false) {
                outfit = await _Db.GetByID(outfitID);

                if (outfit == null || HasExpired(outfit) == true) {
                    // If the outfit is in DB but not Census, might as well return from DB
                    //      Useful if census is down, or outfit is deleted
                    try {
                        PsOutfit? censusOutfit = await _Census.GetByID(outfitID);
                        if (censusOutfit != null) {
                            outfit = await _Census.GetByID(outfitID);
                            await _Db.Upsert(censusOutfit);
                        }
                    } catch (Exception ex) {
                        if (outfit == null) {
                            throw;
                        } else {
                            _Logger.LogWarning(ex, $"error getting outfit {outfitID} from Census (falling back to DB)");
                        }
                    }
                }

                if (outfit != null) {
                    _Cache.Set(key, outfit, new MemoryCacheEntryOptions() {
                        SlidingExpiration = TimeSpan.FromMinutes(20)
                    });
                } else {
                    _Cache.Set(key, outfit, new MemoryCacheEntryOptions() {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1)
                    });
                }
            }

            return outfit;
        }

        /// <summary>
        ///     Get a list of all outfits that have a tag (case-insensitive)
        /// </summary>
        /// <remarks>
        ///     If an empty string, or a string with more than 4 characters is passed, an empty list is returned
        ///     <br/>
        ///     A list is returned, as while only one active outfit can have a tag, deleted outfits may have the tag
        /// </remarks>
        /// <param name="tag">Tag to search by</param>
        /// <returns>
        ///     A list of all <see cref="PsOutfit"/> with <see cref="PsOutfit.Tag"/> of <paramref name="tag"/>
        /// </returns>
        public async Task<List<PsOutfit>> GetByTag(string tag) {
            if (tag == "" || tag.Length > 4) {
                return new List<PsOutfit>();
            }

            return await _Db.GetByTag(tag);
        }

        /// <summary>
        ///     Get a bunch of outfits at once by ID
        /// </summary>
        /// <remarks>
        ///     It is assumed that all outfits are current, and none have expired
        /// </remarks>
        /// <param name="IDs">List of IDs to get</param>
        /// <returns>
        ///     A list of <see cref="PsOutfit"/>s, each on with a <see cref="PsOutfit.ID"/>
        ///     that is an element of <paramref name="IDs"/>
        /// </returns>
        public async Task<List<PsOutfit>> GetByIDs(List<string> IDs) {

            List<PsOutfit> outfits = new List<PsOutfit>(IDs.Count);

            int total = IDs.Count;
            int inCache = 0;

            foreach (string ID in IDs.ToList()) {
                string cacheKey = string.Format(_CacheKeyID, ID);

                if (_Cache.TryGetValue(cacheKey, out PsOutfit? outfit) == true && outfit != null) {
                    outfits.Add(outfit);
                    IDs.Remove(ID);
                    ++inCache;
                }
            }

            int inDb = 0;
            List<PsOutfit> dbOutfits = await _Db.GetByIDs(IDs);
            foreach (PsOutfit outfit in dbOutfits) {
                outfits.Add(outfit);
                IDs.Remove(outfit.ID);
                ++inDb;
            }

            int inCensus = 0;
            List<PsOutfit> censusOutfits = await _Census.GetByIDs(IDs);
            foreach (PsOutfit outfit in censusOutfits) {
                outfits.Add(outfit);
                IDs.Remove(outfit.ID);
                ++inCensus;
            }

            foreach (PsOutfit outfit in outfits) {
                string cacheKey = string.Format(_CacheKeyID, outfit.ID);
                _Cache.Set(cacheKey, outfit, new MemoryCacheEntryOptions() {
                    SlidingExpiration = TimeSpan.FromMinutes(20)
                });
            }

            //_Logger.LogDebug($"Found {inCache + inDb + inCensus}/{total} outfits. "
                //+ $"In cache: {inCache}, db: {inDb}, census: {inCensus}, left: {IDs.Count}");

            return outfits;
        }

        /// <summary>
        ///     Search for outfits based on their tag and name (case-insensitive)
        /// </summary>
        /// <remarks>
        ///     A Census lookup is done. The census lookup is cancelled if there are database entries, and the lookup takes more than 600ms.
        ///     But if there are no DB entries found that contain the tag or name, the Census lookup runs fully
        /// </remarks>
        /// <param name="tagOrName">Tag or name to search</param>
        /// <returns>
        ///     A list of all outfits that have a <see cref="PsOutfit.Tag"/> of <paramref name="tagOrName"/>
        ///     or contain <paramref name="tagOrName"/> in <see cref="PsOutfit.Name"/>
        /// </returns>
        /// <exception cref="ArgumentException">
        ///     If <paramref name="tagOrName"/>'s Length is less than 1
        /// </exception>
        public async Task<List<PsOutfit>> Search(string tagOrName) {
            if (tagOrName.Length < 1) {
                throw new ArgumentException($"{nameof(tagOrName)} must be at least one character");
            }

            bool tagOnly = tagOrName.StartsWith('[');

            Stopwatch timer = Stopwatch.StartNew();

            List<PsOutfit> outfits = new List<PsOutfit>();

            // Starting a search with a [ is a tag only search
            if (tagOnly == false) {
                outfits.AddRange(await _Db.SearchByName(tagOrName));

                // Don't search for tags if the search param is over 4 chars, as outfit tags are only 4 chars
                if (tagOrName.Length <= 4) {
                    outfits.AddRange(await _Db.GetByTag(tagOrName));
                }
            } else {
                string tag = tagOrName[1..]; // fancy new range operator :o

                outfits.AddRange(await _Db.GetByTag(tag));
            }

            long dbLookup = timer.ElapsedMilliseconds;
            timer.Restart();

            bool hasDbResults = outfits.Count > 0;

            bool censusCancelled = true;

            List<PsOutfit> census = new List<PsOutfit>();

            // Setup a task that will be cancelled in the timeout period given, to ensure this method is fast
            Task<List<PsOutfit>> nameWrapper = Task.Run(() => _Census.SearchByName(tagOrName));
            Task<PsOutfit?> tagWrapper = (tagOnly == false) ? Task.Run(() => _Census.GetByTag(tagOrName)) : Task.FromResult<PsOutfit?>(null);

            // If the DB had results, set a timeout so this method is kinda quick for that speedy lookup time, 
            //      else let the search run the full time
            if (hasDbResults == true) {
                censusCancelled = nameWrapper.Wait(TimeSpan.FromMilliseconds(SEARCH_CENSUS_TIMEOUT_MS)) == false;
                if (censusCancelled == false) {
                    census.AddRange(nameWrapper.Result);
                }

                censusCancelled = tagWrapper.Wait(TimeSpan.FromMilliseconds(SEARCH_CENSUS_TIMEOUT_MS)) == false;
                if (censusCancelled == false && tagWrapper.Result != null) {
                    census.Add(tagWrapper.Result);
                }
            } else {
                census.AddRange(await nameWrapper);

                PsOutfit? censusTagged = await tagWrapper;
                if (censusTagged != null) {
                    census.Add(censusTagged);
                }
            }

            foreach (PsOutfit outfit in census) {
                outfits.Add(outfit);
            }

            // If any outfits were found, get those that don't exist in the DB and upsert them
            if (census.Count > 0) {
                new Thread(async () => {
                    try {
                        int inserted = 0;

                        foreach (PsOutfit outfit in census) {
                            PsOutfit? d = outfits.FirstOrDefault(iter => iter.ID == outfit.ID);
                            if (d == null) {
                                await _Db.Upsert(outfit);
                                ++inserted;
                            }
                        }

                        _Logger.LogDebug($"Inserted {inserted} new outfits from the {census.Count} found in Census");
                    } catch (Exception ex) {
                        _Logger.LogError(ex, $"Error while performing update on {census.Count} outfits");
                    }
                }).Start();
            }

            long censusLookup = timer.ElapsedMilliseconds;

            outfits = outfits.DistinctBy(iter => iter.ID).ToList();

            _Logger.LogDebug($"Timings to lookup '{tagOrName}':\n"
                + $"\tDB search: {dbLookup}ms\n"
                + $"\tCensus search: {censusLookup}ms {(censusCancelled ? "(cancelled)" : "")}"
            );

            return outfits;
        }

        /// <summary>
        ///     Get members of an outfit. This is not stored in the DB, and results in a Census call if
        ///     the result is not cached
        /// </summary>
        /// <param name="outfitID">ID of the outfit</param>
        /// <returns>
        ///     A list containing the characters that are in an outfit
        /// </returns>
        public async Task<List<OutfitMember>> GetMembers(string outfitID) {
            string cacheKey = string.Format(CACHE_KEY_MEMBERS, outfitID);

            if (_Cache.TryGetValue(cacheKey, out List<OutfitMember> members) == false) {
                members = await _Census.GetMembers(outfitID);

                _Cache.Set(cacheKey, members, new MemoryCacheEntryOptions() {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
                });
            }

            return members;
        }

        private bool HasExpired(PsOutfit outfit) {
            bool expired = DateTime.UtcNow > outfit.LastUpdated + TimeSpan.FromDays(3);
            return expired;
        }

    }
}
