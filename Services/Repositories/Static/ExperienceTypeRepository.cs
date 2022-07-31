using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using watchtower.Models.Census;
using watchtower.Services.Census;
using watchtower.Services.Db;

namespace watchtower.Services.Repositories {

    public class ExperienceTypeRepository : BaseStaticRepository<ExperienceType> {

        public ExperienceTypeRepository(ILoggerFactory loggerFactory,
                IStaticCollection<ExperienceType> census, IStaticDbStore<ExperienceType> db,
                IMemoryCache cache)
            : base(loggerFactory, census, db, cache) { }

    }
}
