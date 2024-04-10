using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Code.Constants;
using watchtower.Models;
using watchtower.Models.Census;
using watchtower.Services.Census;
using watchtower.Services.Db;

namespace watchtower.Services.Repositories {

    public class MapRepository {

        private readonly ILogger<MapRepository> _Logger;
        private readonly IMemoryCache _Cache;

        private const string KEY_FACILITIES = "Ledger.Facilities";
        private const string KEY_LINKS = "Ledger.Links";
        private const string KEY_HEXES = "Ledger.Hexes";

        private readonly IMapDbStore _MapDb;
        private readonly IFacilityDbStore _FacilityDb;
        private readonly MapCollection _MapCensus;

        private readonly Dictionary<short, PsWorldMap> _Maps = new Dictionary<short, PsWorldMap>();

        public MapRepository(ILogger<MapRepository> logger,
            IMemoryCache cache, IMapDbStore mapDb,
            IFacilityDbStore facDb, MapCollection mapColl) {

            _Logger = logger;
            _Cache = cache;

            _MapDb = mapDb ?? throw new ArgumentNullException(nameof(mapDb));
            _FacilityDb = facDb ?? throw new ArgumentNullException(nameof(facDb));
            _MapCensus = mapColl ?? throw new ArgumentNullException(nameof(mapColl));
        }

        /// <summary>
        ///     Set the faction that owns a facility
        /// </summary>
        /// <param name="worldID">ID of the world</param>
        /// <param name="zoneID">Zone the facility is in</param>
        /// <param name="facilityID">ID of the facility</param>
        /// <param name="factionID">ID of the faction</param>
        public void Set(short worldID, uint zoneID, int facilityID, short factionID) {
            if (_Maps.TryGetValue(worldID, out PsWorldMap? map) == false) {
                map = new PsWorldMap();
                map.WorldID = worldID;

                _Maps.Add(worldID, map);
            }

            PsZone zone = map.GetZone(zoneID);
            if (zone.GetFacilityOwner(facilityID) == null) {
                //_Logger.LogDebug($"Adding new facilityID {facilityID} in world {worldID}, zone {zoneID}, owner {factionID}");
            }
            zone.SetFacilityOwner(facilityID, factionID);
        }

        /// <summary>
        ///     Get the zone map of a world
        /// </summary>
        /// <param name="worldID">ID of the world to get the zone of</param>
        /// <param name="zoneID">Zone ID</param>
        /// <returns></returns>
        public PsZone? GetZone(short worldID, uint zoneID) {
            if (_Maps.TryGetValue(worldID, out PsWorldMap? map) == false) {
                map = new PsWorldMap();
                map.WorldID = worldID;

                _Maps.Add(worldID, map);
            }

            return map.GetZone(zoneID);
        }

        /// <summary>
        ///     Get the owner of a zone
        /// </summary>
        /// <param name="worldID">World ID</param>
        /// <param name="zoneID">Zone ID to get the owner of</param>
        /// <returns>The faction ID that currently owns a zone, or <c>null</c> if there is no owner</returns>
        public short? GetZoneMapOwner(short worldID, uint zoneID) {
            PsZone? zone = GetZone(worldID, zoneID);
            if (zone == null) {
                return null;
            }

            List<PsFacilityOwner> facs = zone.GetFacilities();

            //_Logger.LogTrace($"GetZoneMapOwner = {worldID}:{zoneID} => using {facs.Count} regions");
            int total = facs.Count;

            Dictionary<short, int> counts = new();
            int found = 0;

            foreach (PsFacilityOwner region in facs) {
                if (counts.ContainsKey(region.Owner) == false) {
                    counts.Add(region.Owner, 0);
                }

                ++counts[region.Owner];
                ++found;
            }

            //_Logger.LogTrace($"GetZoneMapOwner = {worldID}:{zoneID}, have {found} regions in zone => {string.Join(", ", counts.Select(kvp => kvp.Key + ": " + kvp.Value))}");

            if (total > 10 && counts.Count > 0) {
                KeyValuePair<short, int> majority = counts.ToList().OrderByDescending(iter => iter.Value).First();

                // if one faction a number of facilities equal to the number of facilities in a zone, they own it
                // Esamir has 2 disabled regions
                if (majority.Value >= total - 2) {
                    return majority.Key;
                }
            }

            return null;
        }

        /// <summary>
        ///     Get what <c>UnstableState</c> a zone is
        /// </summary>
        /// <param name="worldID">World ID</param>
        /// <param name="zoneID">Zone ID</param>
        /// <returns>
        ///     The <see cref="UnstableState"/> of the zone. If no zone is found, <see cref="UnstableState.LOCKED"/> is returned
        /// </returns>
        public UnstableState GetUnstableState(short worldID, uint zoneID) {
            PsZone? zone = GetZone(worldID, zoneID);
            if (zone == null) {
                return UnstableState.LOCKED;
            }

            List<PsFacilityOwner> facs = zone.GetFacilities();

            int total = facs.Count;
            //_Logger.LogDebug($"GetUnstableState = {worldID}:{zoneID} => using {facs.Count} regions");

            Dictionary<short, int> counts = new();
            int found = 0;

            foreach (PsFacilityOwner region in facs) {
                if (counts.ContainsKey(region.Owner) == false) {
                    counts.Add(region.Owner, 0);
                }

                ++counts[region.Owner];
                ++found;
            }

            //_Logger.LogInformation($"GetUnstableState = {worldID}:{zoneID}, have {found} regions in zone => {string.Join(", ", counts.Select(kvp => kvp.Key + ": " + kvp.Value))}\n\t{string.Join(", ", facs.Select(iter => iter.FacilityID))}");

            if (counts.TryGetValue(0, out int value) == true) {
                if (value > (total / 2)) {
                    return UnstableState.SINGLE_LANE;
                }

                if (value > 10) {
                    return UnstableState.DOUBLE_LANE;
                }
            }

            if (GetZoneMapOwner(worldID, zoneID) == null) {
                return UnstableState.UNLOCKED;
            }

            return UnstableState.LOCKED;
        }

        /// <summary>
        ///     Get the <see cref="PsFacility"/>s
        /// </summary>
        public async Task<List<PsFacility>> GetFacilities() {
            if (_Cache.TryGetValue(KEY_FACILITIES, out List<PsFacility>? facs) == false || facs == null) {
                facs = await _FacilityDb.GetAll();

                _Cache.Set(KEY_FACILITIES, facs, new MemoryCacheEntryOptions() {
                    Priority = CacheItemPriority.NeverRemove
                });
            }

            return facs;
        }

        /// <summary>
        ///     Get the <see cref="PsFacilityLink"/>s
        /// </summary>
        public async Task<List<PsFacilityLink>> GetFacilityLinks() {
            if (_Cache.TryGetValue(KEY_LINKS, out List<PsFacilityLink>? links) == false || links == null) {
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

        /// <summary>
        ///     Get all the <see cref="PsMapHex"/>es
        /// </summary>
        public async Task<List<PsMapHex>> GetHexes() {
            if (_Cache.TryGetValue(KEY_HEXES, out List<PsMapHex>? hexes) == false || hexes == null) {
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
