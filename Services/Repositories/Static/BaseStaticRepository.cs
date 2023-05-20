
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using watchtower.Models;
using watchtower.Services.Census;
using watchtower.Services.Db;

namespace watchtower.Services.Repositories {

    public class BaseStaticRepository<T> : IStaticRepository<T> where T : IKeyedObject {

        internal readonly ILogger _Logger;
        internal readonly IStaticCollection<T> _Census;
        internal readonly IStaticDbStore<T> _Db;

        internal readonly IMemoryCache _Cache;

        private readonly string CACHE_KEY_ALL;
        private readonly string CACHE_KEY_MAP;

        internal BaseStaticRepository(ILoggerFactory loggerFactory,
            IStaticCollection<T> census, IStaticDbStore<T> db,
            IMemoryCache cache) {

            _Logger = loggerFactory.CreateLogger($"StaticRepository<{typeof(T).Name}>");
            _Census = census ?? throw new ArgumentNullException(nameof(census));
            _Db = db ?? throw new ArgumentNullException(nameof(db));
            _Cache = cache;

            CACHE_KEY_ALL = $"{typeof(T).Name}.All";
            CACHE_KEY_MAP = $"{typeof(T).Name}.Map";
        }

        /// <summary>
        ///     Get all the static data in this repository
        /// </summary>
        /// <returns>
        ///     A list of static PS2 data
        /// </returns>
        public async Task<List<T>> GetAll() {
            if (_Cache.TryGetValue(CACHE_KEY_ALL, out List<T> entries) == false) {
                entries = await _Db.GetAll();

                if (entries.Count == 0) {
                    List<T> censusEntries = await _Census.GetAll();
                    if (censusEntries.Count > 0) {
                        foreach (T entry in censusEntries) {
                            await _Db.Upsert(entry);
                        }
                    }
                }

                _Cache.Set(CACHE_KEY_ALL, entries, new MemoryCacheEntryOptions() {
                    SlidingExpiration = TimeSpan.FromMinutes(30)
                });
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

        public async Task<List<T>> GetByIDs(IEnumerable<int> IDs) {
            return (await GetAll()).Where(iter => IDs.Contains(iter.ID)).ToList();
        }

    }

}