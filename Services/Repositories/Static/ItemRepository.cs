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

    public class ItemRepository : BaseStaticRepository<PsItem> {

        public ItemRepository(ILoggerFactory loggerFactory,
            IStaticCollection<PsItem> census, IStaticDbStore<PsItem> db,
            IMemoryCache cache)
            : base(loggerFactory, census, db, cache) { }

    }
}
