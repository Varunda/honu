
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using watchtower.Models;
using watchtower.Services.Census;
using watchtower.Services.Db;
using watchtower.Services.Repositories.Static;

namespace watchtower.Services.Repositories {

    public class BaseStaticRepository<T> : IRefreshableRepository, IStaticRepository<T> where T : IKeyedObject {

        internal readonly ILogger _Logger;
        internal readonly IStaticCollection<T> _Census;
        internal readonly IStaticDbStore<T> _Db;

        internal readonly IMemoryCache _Cache;

        private readonly string CACHE_KEY_ALL;
        private readonly string CACHE_KEY_MAP;

        private readonly string TypeName;

        internal BaseStaticRepository(ILoggerFactory loggerFactory,
            IStaticCollection<T> census, IStaticDbStore<T> db,
            IMemoryCache cache) {

            TypeName = typeof(T).Name;

            _Logger = loggerFactory.CreateLogger($"watchtower.Services.Repositories.StaticRepository<{TypeName}>");
            _Census = census ?? throw new ArgumentNullException(nameof(census));
            _Db = db ?? throw new ArgumentNullException(nameof(db));
            _Cache = cache;

            CACHE_KEY_ALL = $"{TypeName}.All";
            CACHE_KEY_MAP = $"{TypeName}.Map";
        }

        /// <summary>
        ///     Get all the static data in this repository
        /// </summary>
        /// <returns>
        ///     A list of static PS2 data
        /// </returns>
        public async Task<List<T>> GetAll() {
            if (_Cache.TryGetValue(CACHE_KEY_ALL, out List<T> entries) == false) {
                string method = "db";
                _Logger.LogDebug($"loading static data, not cached [TypeName={TypeName}] [cacheKey={CACHE_KEY_ALL}]");
                entries = await _Db.GetAll();

                if (entries.Count == 0) {
                    _Logger.LogInformation($"loaded 0 entries from DB, loading from census [TypeName={TypeName}]");
                    List<T> censusEntries = await _Census.GetAll();
                    method = "census";
                    if (censusEntries.Count > 0) {
                        foreach (T entry in censusEntries) {
                            await _Db.Upsert(entry);
                        }
                    }
                }

                _Cache.Set(CACHE_KEY_ALL, entries, new MemoryCacheEntryOptions() {
                    SlidingExpiration = TimeSpan.FromMinutes(30)
                });

                _Logger.LogDebug($"loaded static data [TypeName={TypeName}] [method={method}] [count={entries.Count}]");
            }

            return entries;
        }

        /// <summary>
        ///     Get a single <typeparamref name="T"/> by its <see cref="IKeyedObject.ID"/>
        /// </summary>
        /// <param name="ID">ID of the <typeparamref name="T"/> to get</param>
        /// <returns>
        ///     The <typeparamref name="T"/> with <see cref="IKeyedObject.ID"/> of <paramref name="ID"/>,
        ///     or <c>null</c> if it does not exist
        /// </returns>
        public async Task<T?> GetByID(int ID) {
            // In cases where there are thousands of entries, such as directives and objectives,
            //      using .FirstOrDefault() on a list can take too long, esp when iterating thru a list
            //      and getting the objective by ID, which can take an O(n) operation into O(n^2),
            //      not good. Having a lookup be O(1) too is just nicer in general and more inline
            //      if what you'd expect
            if (_Cache.TryGetValue(CACHE_KEY_MAP, out Dictionary<int, T> entries) == false) {
                List<T> all = await GetAll();
                entries = new Dictionary<int, T>(all.Count);

                foreach (T iter in all) {
                    entries.Add(iter.ID, iter);
                }

                _Cache.Set(CACHE_KEY_MAP, entries, new MemoryCacheEntryOptions() {
                    SlidingExpiration = TimeSpan.FromMinutes(30)
                });
            }

            entries.TryGetValue(ID, out T? entry);
            return entry;
        }

        /// <summary>
        ///     Get multiple entries based on its ID
        /// </summary>
        /// <param name="IDs">List of IDs to get the data of</param>
        /// <returns>
        ///     A list of <typeparamref name="T"/> that contain a <see cref="IKeyedObject.ID"/>
        ///     within <paramref name="IDs"/>
        /// </returns>
        public async Task<List<T>> GetByIDs(IEnumerable<int> IDs) {
            return (await GetAll()).Where(iter => IDs.Contains(iter.ID)).ToList();
        }

        /// <summary>
        ///     Remove the cached data, forcing a Census call the next time 
        /// </summary>
        public void FlushCache() {
            _Cache.Remove(CACHE_KEY_ALL);
            _Cache.Remove(CACHE_KEY_MAP);
        }

        /// <summary>
        ///     refresh the data within this repository
        /// </summary>
        /// <param name="cancel">async cancellation token</param>
        public async Task Refresh(CancellationToken cancel) {
            Stopwatch timer = Stopwatch.StartNew();

            List<T> census = await _Census.GetAll(cancel);
            if (census.Count == 0) {
                return;
            }

            long censusMs = timer.ElapsedMilliseconds; timer.Restart();

            foreach (T t in census) {
                await _Db.Upsert(t);
            }

            long dbMs = timer.ElapsedMilliseconds; timer.Restart();

            _Logger.LogInformation($"Refreshed {TypeName} with {census.Count} entries in {censusMs + dbMs}ms. [Census={censusMs}ms] [Db={dbMs}ms]");

            _Cache.Remove(CACHE_KEY_ALL);
            _Cache.Remove(CACHE_KEY_MAP);
        }

    }

}