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

    public class DirectiveTreeCategoryRepository : BaseStaticRepository<DirectiveTreeCategory> {

        public DirectiveTreeCategoryRepository(ILoggerFactory loggerFactory,
            IStaticCollection<DirectiveTreeCategory> census, IStaticDbStore<DirectiveTreeCategory> db,
            IMemoryCache cache)
            : base(loggerFactory, census, db, cache) { }

    }

}