using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using watchtower.Models.Census;
using watchtower.Services.Census;
using watchtower.Services.Db;

namespace watchtower.Services.Repositories {

    public class ItemCategoryRepository : BaseStaticRepository<ItemCategory> {

        public ItemCategoryRepository(ILoggerFactory loggerFactory,
                IStaticCollection<ItemCategory> census, IStaticDbStore<ItemCategory> db,
                IMemoryCache cache)
            : base(loggerFactory, census, db, cache) {
        }

    }
}
