using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using watchtower.Models.Census;
using watchtower.Services.Census;
using watchtower.Services.Db;

namespace watchtower.Services.Repositories {

    public class AchievementRepository : BaseStaticRepository<Achievement> {

        public AchievementRepository(ILoggerFactory loggerFactory,
            IStaticCollection<Achievement> census, IStaticDbStore<Achievement> db,
            IMemoryCache cache)
            : base(loggerFactory, census, db, cache) { }

    }
}
