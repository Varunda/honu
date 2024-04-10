using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using watchtower.Models.Census;
using watchtower.Services.Census;
using watchtower.Services.Db;

namespace watchtower.Services.Repositories {

    public class ObjectiveRepository : BaseStaticRepository<PsObjective> {

        private readonly string CACHE_KEY_GROUP_MAP = "Objective.Group.Map";

        public ObjectiveRepository(ILoggerFactory loggerFactory,
            IStaticCollection<PsObjective> census, IStaticDbStore<PsObjective> db, 
            IMemoryCache cache)
            : base(loggerFactory, census, db, cache) { }

        public async Task<PsObjective?> GetByGroupID(int groupID) {
            if (_Cache.TryGetValue(CACHE_KEY_GROUP_MAP, out Dictionary<int, PsObjective>? map) == false || map == null) {
                List<PsObjective> all = await GetAll();
                map = new Dictionary<int, PsObjective>(all.Count);

                foreach (PsObjective iter in all) {
                    if (map.ContainsKey(iter.GroupID)) {
                        _Logger.LogWarning($"GroupID {iter.GroupID} set twice");
                    } else {
                        map.Add(iter.GroupID, iter);
                    }
                }

                _Cache.Set(CACHE_KEY_GROUP_MAP, map, new MemoryCacheEntryOptions() {
                    SlidingExpiration = TimeSpan.FromMinutes(30)
                });
            }

            map.TryGetValue(groupID, out PsObjective? obj);
            return obj;
        }

    }

}
