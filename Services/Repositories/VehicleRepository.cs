using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using watchtower.Models.Census;
using watchtower.Services.Census;
using watchtower.Services.Db;

namespace watchtower.Services.Repositories {

    public class VehicleRepository : BaseStaticRepository<PsVehicle> {

        public VehicleRepository(ILoggerFactory loggerFactory,
                IStaticCollection<PsVehicle> census, IStaticDbStore<PsVehicle> db,
                IMemoryCache cache)
            : base(loggerFactory, census, db, cache) {
        }

    }
}
