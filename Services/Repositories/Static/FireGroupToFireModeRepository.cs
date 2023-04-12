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

    public class FireGroupToFireModeRepository : IStaticRepository<FireGroupToFireMode> {

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

        public async Task<List<FireGroupToFireMode>> GetAll() {
            if (_Cache.TryGetValue(CACHE_KEY_ALL, out List<FireGroupToFireMode>? modes) == false) {
                modes = await _Db.GetAll();

                if (modes.Count == 0) {
                    List<FireGroupToFireMode> censusEntries = await _Census.GetAll();
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

        public Task<FireGroupToFireMode?> GetByID(int ID) {
            throw new NotImplementedException();
        }

        public async Task<List<FireGroupToFireMode>> GetByFireGroupID(int fireGroupID) {
            return (await GetAll()).Where(iter => iter.FireGroupID == fireGroupID).ToList();
        }

        public async Task<List<FireGroupToFireMode>> GetByFireModeID(int fireModeID) {
            List<FireGroupToFireMode> all = await GetAll();
            return all.Where(iter => iter.FireModeID == fireModeID).ToList();
        }

    }
}
