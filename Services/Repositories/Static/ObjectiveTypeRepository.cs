using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using watchtower.Models.Census;
using watchtower.Services.Census;
using watchtower.Services.Db;

namespace watchtower.Services.Repositories {

    public class ObjectiveTypeRepository : BaseStaticRepository<ObjectiveType> {

        public ObjectiveTypeRepository(ILoggerFactory loggerFactory,
            IStaticCollection<ObjectiveType> census, IStaticDbStore<ObjectiveType> db, 
            IMemoryCache cache)
            : base(loggerFactory, census, db, cache) { }

    }

}
