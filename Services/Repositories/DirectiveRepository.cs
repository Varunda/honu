using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using watchtower.Models.Census;
using watchtower.Models.Db;
using watchtower.Services.Census;
using watchtower.Services.Db;

namespace watchtower.Services.Repositories {

    public class DirectiveRepository {

        private readonly ILogger<DirectiveRepository> _Logger;
        private readonly IMemoryCache _Cache;

        private readonly DirectiveDbStore _Db;

        private const string CACHE_KEY = "Directives.All"; 

        public DirectiveRepository(ILogger<DirectiveRepository> logger,
            IMemoryCache cache, DirectiveDbStore db) {

            _Logger = logger;
            _Cache = cache;

            _Db = db;
        }

        public async Task<List<PsDirective>> GetAll() {
            if (_Cache.TryGetValue(CACHE_KEY, out List<PsDirective> dirs) == false) {
                dirs = await _Db.GetAll();

                if (dirs.Count > 0) {
                    _Cache.Set(CACHE_KEY, dirs, new MemoryCacheEntryOptions() {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(2)
                    });
                }
            }

            return dirs;
        }

        public async Task<PsDirective?> GetByID(int ID) {
            List<PsDirective> all = await GetAll();

            return all.FirstOrDefault(iter => iter.ID == ID);
        }

        public async Task<List<PsDirective>> GetByTreeID(int treeID) {
            List<PsDirective> all = await GetAll();

            return all.Where(iter => iter.TreeID == treeID).ToList();
        }

    }

}