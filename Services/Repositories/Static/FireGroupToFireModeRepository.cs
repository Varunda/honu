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
using watchtower.Services.Repositories.Static;

namespace watchtower.Services.Repositories {

    public class FireGroupToFireModeRepository : IStaticRepository<FireGroupToFireMode>, IRefreshableRepository {

        private readonly ILogger<FireGroupToFireModeRepository> _Logger;
        private readonly IMemoryCache _Cache;

        private const string CACHE_KEY_ALL = "FireGroupToFireMode.All";

        private readonly IStaticCollection<FireGroupToFireMode> _Census;
        private readonly IStaticDbStore<FireGroupToFireMode> _Db;

        public FireGroupToFireModeRepository(ILogger<FireGroupToFireModeRepository> logger, IMemoryCache cache,
            IStaticCollection<FireGroupToFireMode> census, IStaticDbStore<FireGroupToFireMode> db) {

            _Logger = logger;
            _Cache = cache;

            _Census = census;
            _Db = db;
        }

        /// <summary>
        ///     Get all xtypes that exist
        /// </summary>
        public async Task<List<FireGroupToFireMode>> GetAll(CancellationToken cancel = default) {
            if (_Cache.TryGetValue(CACHE_KEY_ALL, out List<FireGroupToFireMode>? modes) == false || modes == null) {
                modes = await _Db.GetAll(cancel);

                if (modes.Count == 0) {
                    List<FireGroupToFireMode> censusEntries = await _Census.GetAll(cancel);
                    if (censusEntries.Count > 0) {
                        foreach (FireGroupToFireMode entry in censusEntries) {
                            await _Db.Upsert(entry);
                        }
                    }
                    modes = censusEntries;
                }

                _Cache.Set(CACHE_KEY_ALL, modes, new MemoryCacheEntryOptions() {
                    SlidingExpiration = TimeSpan.FromMinutes(30)
                });
            }

            return modes;
        }

        /// <summary>
        ///     DO NOT USE!!!
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task<FireGroupToFireMode?> GetByID(int ID) {
            _Logger.LogError($"{nameof(GetByID)} is not implemented for a {nameof(FireGroupToFireModeRepository)} as there is not a 1:1 mapping (no primary key)! "
                + $"You want to use {nameof(GetByFireGroupID)} or {nameof(GetByFireModeID)} instead");
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Get all entries with a matching fire group
        /// </summary>
        public async Task<List<FireGroupToFireMode>> GetByFireGroupID(int fireGroupID) {
            return (await GetAll()).Where(iter => iter.FireGroupID == fireGroupID).ToList();
        }

        /// <summary>
        ///     get all entries with a matching fire mode
        /// </summary>
        /// <param name="fireModeID"></param>
        /// <returns></returns>
        public async Task<List<FireGroupToFireMode>> GetByFireModeID(int fireModeID) {
            List<FireGroupToFireMode> all = await GetAll();
            return all.Where(iter => iter.FireModeID == fireModeID).ToList();
        }

        /// <summary>
        ///     Get all entries with a matching firemode in <paramref name="ids"/>
        /// </summary>
        /// <param name="ids">List of IDs to include</param>
        /// <returns></returns>
        public async Task<List<FireGroupToFireMode>> GetByFireModes(IEnumerable<int> ids) {
            List<FireGroupToFireMode> entries = new(ids.Count());
            foreach (int id in ids) {
                List<FireGroupToFireMode> modes = await GetByFireModeID(id);
                if (modes.Count > 0) {
                    entries.Add(modes.ElementAt(0));
                }
            }
            return entries;
        }

        public async Task Refresh(CancellationToken cancel) {
            Stopwatch timer = Stopwatch.StartNew();

            List<FireGroupToFireMode> census = await _Census.GetAll(cancel);
            if (census.Count == 0) {
                return;
            }

            long censusMs = timer.ElapsedMilliseconds; timer.Restart();

            foreach (FireGroupToFireMode t in census) {
                await _Db.Upsert(t);
            }

            long dbMs = timer.ElapsedMilliseconds; timer.Restart();

            _Logger.LogInformation($"Refreshed {nameof(FireGroupToFireMode)} with {census.Count} entries in {censusMs + dbMs}ms. [Census={censusMs}ms] [Db={dbMs}ms]");

            _Cache.Remove(CACHE_KEY_ALL);
        }

    }
}
