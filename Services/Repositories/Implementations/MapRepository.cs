using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models.Census;
using watchtower.Services.Census;
using watchtower.Services.Db;

namespace watchtower.Services.Repositories.Implementations {

    public class MapRepository : IMapRepository {

        private readonly ILogger<MapRepository> _Logger;
        private readonly IMemoryCache _Cache;

        private const string KEY_FACILITIES = "Ledger.Facilities";
        private const string KEY_LINKS = "Ledger.Links";
        private const string KEY_HEXES = "Ledger.Hexes";

        private readonly IMapDbStore _MapDb;
        private readonly IFacilityDbStore _FacilityDb;
        private readonly IMapCollection _MapCensus;

        public MapRepository(ILogger<MapRepository> logger,
            IMemoryCache cache, IMapDbStore mapDb,
            IFacilityDbStore facDb, IMapCollection mapColl) {

            _Logger = logger;
            _Cache = cache;

            _MapDb = mapDb ?? throw new ArgumentNullException(nameof(mapDb));
            _FacilityDb = facDb ?? throw new ArgumentNullException(nameof(facDb));
            _MapCensus = mapColl ?? throw new ArgumentNullException(nameof(mapColl));
        }

        public async Task<List<PsFacility>> GetFacilities() {
            if (_Cache.TryGetValue(KEY_FACILITIES, out List<PsFacility> facs) == false) {
                facs = await _FacilityDb.GetAll();

                _Cache.Set(KEY_FACILITIES, facs, new MemoryCacheEntryOptions() {
                    Priority = CacheItemPriority.NeverRemove
                });
            }

            return facs;
        }

        public async Task<List<PsFacilityLink>> GetFacilityLinks() {
            if (_Cache.TryGetValue(KEY_LINKS, out List<PsFacilityLink> links) == false) {
                links = await _MapDb.GetFacilityLinks();

                if (links.Count == 0) {
                    links = await _MapCensus.GetFacilityLinks();

                    foreach (PsFacilityLink link in links) {
                        await _MapDb.UpsertLink(link);
                    }
                }

                _Cache.Set(KEY_LINKS, links, new MemoryCacheEntryOptions() {
                    Priority = CacheItemPriority.NeverRemove
                });
            }

            return links;
        }

        public async Task<List<PsMapHex>> GetHexes() {
            if (_Cache.TryGetValue(KEY_HEXES, out List<PsMapHex> hexes) == false) {
                hexes = await _MapDb.GetHexes();

                if (hexes.Count == 0) {
                    hexes = await _MapCensus.GetHexes();

                    foreach (PsMapHex hex in hexes) {
                        await _MapDb.UpsertHex(hex);
                    }
                }

                _Cache.Set(KEY_HEXES, hexes, new MemoryCacheEntryOptions() {
                    Priority = CacheItemPriority.NeverRemove
                });
            }

            return hexes;
        }

    }
}
