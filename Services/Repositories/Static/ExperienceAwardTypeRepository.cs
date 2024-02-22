using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using watchtower.Models.Census;
using watchtower.Services.Census;
using watchtower.Services.Db;

namespace watchtower.Services.Repositories.Static {

    public class ExperienceAwardTypeRepository : BaseStaticRepository<ExperienceAwardType> {

        public ExperienceAwardTypeRepository(ILoggerFactory loggerFactory,
                IStaticCollection<ExperienceAwardType> census, IStaticDbStore<ExperienceAwardType> db,
                IMemoryCache cache)
            : base(loggerFactory, census, db, cache) { }

    }
}
