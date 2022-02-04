using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models.Census;
using watchtower.Services.Db;

namespace watchtower.Services.Repositories {

    public class FacilityRepository {

        private readonly ILogger<FacilityRepository> _Logger;
        private readonly IMemoryCache _Cache;

        private readonly IFacilityDbStore _Db;

        public FacilityRepository(ILogger<FacilityRepository> logger,
            IFacilityDbStore db, IMemoryCache cache) {

            _Logger = logger;
            _Db = db;
            _Cache = cache;
        }

        public async Task<List<PsFacility>> GetAll() {
            if (_Cache.TryGetValue("Facilities.All", out List<PsFacility> facs) == false) {
                facs = await _Db.GetAll();

                if (facs.Count > 0) {
                    _Cache.Set("Facilities.All", facs, new MemoryCacheEntryOptions() {
                        Priority = CacheItemPriority.NeverRemove
                    });
                }
            }

            return facs;
        }

        public async Task<PsFacility?> GetByID(int facilityID) {
            return (await GetAll()).FirstOrDefault(iter => iter.FacilityID == facilityID);
        }

    }
}
