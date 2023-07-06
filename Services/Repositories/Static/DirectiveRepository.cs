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

    public class DirectiveRepository : BaseStaticRepository<PsDirective> {

        public DirectiveRepository(ILoggerFactory loggerFactory,
            IStaticCollection<PsDirective> census, IStaticDbStore<PsDirective> db,
            IMemoryCache cache)
            : base(loggerFactory, census, db, cache) { }

        public async Task<List<PsDirective>> GetByTreeID(int treeID) {
            List<PsDirective> all = await GetAll();

            return all.Where(iter => iter.TreeID == treeID).ToList();
        }

    }

}