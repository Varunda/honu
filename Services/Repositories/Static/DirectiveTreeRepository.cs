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

    public class DirectiveTreeRepository {

        private readonly ILogger<DirectiveTreeRepository> _Logger;
        private readonly IMemoryCache _Cache;

        private readonly DirectiveTreeDbStore _Db;

        private const string CACHE_KEY = "DirectiveTrees.All"; 

        public DirectiveTreeRepository(ILogger<DirectiveTreeRepository> logger,
            IMemoryCache cache, DirectiveTreeDbStore db) {

            _Logger = logger;
            _Cache = cache;

            _Db = db;
        }

        public async Task<List<DirectiveTree>> GetAll() {
            if (_Cache.TryGetValue(CACHE_KEY, out List<DirectiveTree> dirs) == false) {
                dirs = await _Db.GetAll();

                if (dirs.Count > 0) {
                    _Cache.Set(CACHE_KEY, dirs, new MemoryCacheEntryOptions() {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(2)
                    });
                }
            }

            return dirs;
        }

        public async Task<DirectiveTree?> GetByID(int ID) {
            List<DirectiveTree> all = await GetAll();

            return all.FirstOrDefault(iter => iter.ID == ID);
        }

    }

}