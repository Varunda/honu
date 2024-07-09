using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Constants;
using watchtower.Models;
using watchtower.Models.Api;
using watchtower.Models.Census;

namespace watchtower.Services.Repositories {

    public class VehicleUsageRepository {

        private readonly ILogger<VehicleUsageRepository> _Logger;

        private readonly IMemoryCache _Cache;
        private const string CACHE_KEY = "VehicleUsage.{0}.{1}.{2}"; // {0} -> world ID (0 if null), {1} -> zone ID (0 if null), {2} include vehicles

        private readonly VehicleRepository _VehicleRepository;

        public VehicleUsageRepository(ILogger<VehicleUsageRepository> logger,
            IMemoryCache cache, VehicleRepository vehicleRepository) {

            _Logger = logger;
            _Cache = cache;
            _VehicleRepository = vehicleRepository;
        }

        /// <summary>
        ///     get vehicle usage of online characters, optionally specify a world and zone filter
        /// </summary>
        /// <param name="worldID">ID of the world to filer. can leave default of null to include all worlds</param>
        /// <param name="zoneID">ID of the zone to filter. can leave default of null to include all zones</param>
        /// <param name="includeVehicles">if vehicle data will be populated in <see cref="VehicleUsageEntry.Vehicle"/> or not</param>
        /// <returns>
        ///     a (possibly cached) <see cref="VehicleUsageData"/> that matches the filters passed
        /// </returns>
        public async Task<VehicleUsageData> Get(short? worldID = null, uint? zoneID = null, bool includeVehicles = true) {

            string cacheKey = string.Format(CACHE_KEY, worldID ?? 0, zoneID ?? 0, includeVehicles);

            if (_Cache.TryGetValue(cacheKey, out VehicleUsageData? data) == false || data == null) {
                //_Logger.LogDebug($"vehicle data uncached, generating [cacheKey={cacheKey}] [worldID={worldID}] [zoneID={zoneID}] [includeVehicles={includeVehicles}]");

                Stopwatch timer = Stopwatch.StartNew();

                data = new VehicleUsageData();
                data.WorldID = worldID ?? 0;
                data.ZoneID = zoneID ?? 0;
                data.Timestamp = DateTime.UtcNow;

                // the ! is here cause c# doesn't like that a PsVehicle can become a PsVehicle?, which is fine
                Dictionary<int, PsVehicle?> vehicles = ((includeVehicles == true) ? (await _VehicleRepository.GetAll()).ToDictionary(iter => iter.ID) : [])!;
                lock (CharacterStore.Get().Players) {
                    foreach (KeyValuePair<string, TrackedPlayer> iter in CharacterStore.Get().Players) {
                        TrackedPlayer p = iter.Value;

                        if (p.Online == false
                                || (p.WorldID != worldID && worldID != null)
                                || (p.ZoneID != zoneID && zoneID != null)) {
                            continue;
                        }

                        data.Total += 1;

                        VehicleUsageFaction fact;
                        if (p.TeamID == Faction.NC) {
                            fact = data.Nc;
                        } else if (p.TeamID == Faction.VS) {
                            fact = data.Vs;
                        } else if (p.TeamID == Faction.TR) {
                            fact = data.Tr;
                        } else { 
                            fact = data.Other;
                        }

                        if (fact.Usage.ContainsKey(p.PossibleVehicleID) == false) {
                            VehicleUsageEntry entry = new();
                            entry.VehicleID = p.PossibleVehicleID;
                            if (includeVehicles == true) {
                                entry.Vehicle = vehicles.GetValueOrDefault(p.PossibleVehicleID, null);
                            }

                            if (p.PossibleVehicleID == -1) {
                                entry.VehicleName = "unknown";
                            } else if (p.PossibleVehicleID == 0) {
                                entry.VehicleName = "none";
                            } else {
                                entry.VehicleName = vehicles.GetValueOrDefault(p.PossibleVehicleID, null)?.Name ?? $"<missing {p.PossibleVehicleID}>";
                            }

                            fact.Usage.Add(p.PossibleVehicleID, entry);
                        }

                        ++fact.Total;
                        if (p.PossibleVehicleID != 0) {
                            ++fact.TotalVehicles;
                        }

                        ++fact.Usage[p.PossibleVehicleID].Count;
                    }
                }

                //_Logger.LogDebug($"generated vehicle usage in {timer.ElapsedMilliseconds}ms [worldID={worldID}] [zoneID={zoneID}] [includeVehicles={includeVehicles}]");

                _Cache.Set(CACHE_KEY, data, new MemoryCacheEntryOptions() {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(10)
                });
            }

            return data;
        }

        /// <summary>
        ///     populate the <see cref="VehicleUsageEntry.Vehicle"/> field of a <see cref="VehicleUsageData"/>
        /// </summary>
        /// <param name="data">data to populate</param>
        /// <param name="cancel">cancellation token. defaults to <see cref="CancellationToken.None"/></param>
        /// <returns></returns>
        public async Task AddVehicles(VehicleUsageData data, CancellationToken cancel = default) {
            Dictionary<int, PsVehicle> vehicles = (await _VehicleRepository.GetAll(cancel)).ToDictionary(iter => iter.ID);

            _UpdateVehicleFields(data.Vs, vehicles);
            _UpdateVehicleFields(data.Nc, vehicles);
            _UpdateVehicleFields(data.Tr, vehicles);
            _UpdateVehicleFields(data.Other, vehicles);
        }

        public async Task AddVehicles(List<VehicleUsageData> data, CancellationToken cancel = default) {
            Dictionary<int, PsVehicle> vehicles = (await _VehicleRepository.GetAll(cancel)).ToDictionary(iter => iter.ID);

            foreach (VehicleUsageData datum in data) {
                _UpdateVehicleFields(datum.Vs, vehicles);
                _UpdateVehicleFields(datum.Nc, vehicles);
                _UpdateVehicleFields(datum.Tr, vehicles);
                _UpdateVehicleFields(datum.Other, vehicles);
            }
        }

        private static void _UpdateVehicleFields(VehicleUsageFaction faction, Dictionary<int, PsVehicle> dict) {
            foreach (KeyValuePair<int, VehicleUsageEntry> kvp in faction.Usage) {
                if (kvp.Key == -1) {
                    kvp.Value.VehicleName = "unknown";
                } else if (kvp.Key == 0) {
                    kvp.Value.VehicleName = "none";
                } else {
                    kvp.Value.Vehicle = dict.GetValueOrDefault(kvp.Key);
                    kvp.Value.VehicleName = kvp.Value.Vehicle?.Name ?? $"<missing {kvp.Key}>";
                }
            }
        }



    }
}
