using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using watchtower.Models.Census;
using watchtower.Services.Census;
using watchtower.Services.Db;

namespace watchtower.Services.Repositories {

    public class MetagameEventRepository : BaseStaticRepository<PsMetagameEvent> {

        public MetagameEventRepository(ILoggerFactory loggerFactory,
                IStaticCollection<PsMetagameEvent> census, IStaticDbStore<PsMetagameEvent> db, IMemoryCache cache)
            : base(loggerFactory, census, db, cache) {
        }

    }
}
