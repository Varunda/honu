using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using watchtower.Models.Census;
using watchtower.Services.Census;
using watchtower.Services.Db;

namespace watchtower.Services.Repositories {

    public class ItemTypeRepository : BaseStaticRepository<ItemType> {

        public ItemTypeRepository(ILoggerFactory loggerFactory,
            IStaticCollection<ItemType> census, IStaticDbStore<ItemType> db, IMemoryCache cache)
            : base(loggerFactory, census, db, cache) {

        }

    }
}
