using DaybreakGames.Census.Exceptions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Code;
using watchtower.Code.Constants;
using watchtower.Constants;
using watchtower.Models;
using watchtower.Models.Census;
using watchtower.Services.Census;
using watchtower.Services.Repositories;

namespace watchtower.Services.Hosted.Startup {

    public class ZoneCheckerService : BackgroundService {

        private readonly ILogger<ZoneCheckerService> _Logger;
        private readonly MapCollection _MapCollection;
        private readonly MapRepository _MapRepository;
        private readonly FacilityRepository _FacilityRepository;

        private const string SERVICE_NAME = "zone_periodic_checker";
        private const int RUN_DELAY = 1000 * 60 * 5; // 1000 ms * 60 seconds * 5 minutes

        public ZoneCheckerService(ILogger<ZoneCheckerService> logger,
            MapCollection mapColl, MapRepository mapRepo,
            FacilityRepository facRepo) {

            _Logger = logger;
            _MapCollection = mapColl;
            _MapRepository = mapRepo;
            _FacilityRepository = facRepo;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            _Logger.LogInformation($"{SERVICE_NAME}> starting");

            while (stoppingToken.IsCancellationRequested == false) {
                try {
                    Stopwatch timer = Stopwatch.StartNew();

                    List<PsFacility> facs = await _FacilityRepository.GetAll();

                    foreach (short worldID in World.All) {
                        _Logger.LogDebug($"Getting zone maps for {string.Join(", ", Zone.All.Select(iter => $"{Zone.GetName(iter)}/{iter}"))} for the world {World.GetName(worldID)}");

                        List<PsMap> maps = await GetMaps(worldID);

                        foreach (uint zoneID in Zone.All) {
                            List<PsMap> zoneMap = maps.Where(iter => iter.ZoneID == zoneID).ToList();
                            short? owner = _MapCollection.GetZoneMapOwner(worldID, zoneID, zoneMap);

                            foreach (PsMap region in zoneMap) {
                                PsFacility? fac = facs.FirstOrDefault(iter => iter.RegionID == int.Parse(region.RegionID));
                                if (fac != null) {
                                    _MapRepository.Set(worldID, zoneID, fac.FacilityID, region.FactionID);
                                }
                            }

                            PsZone? zone = _MapRepository.GetZone(worldID, zoneID);
                            if (zone != null) {
                                List<PsFacilityOwner> facts = zone.GetFacilities();
                                //_Logger.LogTrace($"World ID {worldID}, zoneID {zoneID}, got {facts.Count} facilities from {zoneMap.Count}");
                            }

                            lock (ZoneStateStore.Get().Zones) {
                                ZoneState state = ZoneStateStore.Get().GetZone(worldID, zoneID) ?? new() { WorldID = worldID, ZoneID = zoneID };

                                state.IsOpened = (owner == null);

                                ZoneStateStore.Get().SetZone(worldID, zoneID, state);
                            }
                        }
                    }

                    timer.Stop();
                    _Logger.LogInformation($"Finished in {timer.ElapsedMilliseconds}ms");

                    await Task.Delay(RUN_DELAY, stoppingToken);
                } catch (Exception) when (stoppingToken.IsCancellationRequested == true) {
                    _Logger.LogInformation($"{SERVICE_NAME}> stopping");
                } catch (Exception ex) when (stoppingToken.IsCancellationRequested == false) {
                    _Logger.LogError(ex, $"Error in {SERVICE_NAME}");
                }
            }
        }

        private async Task<List<PsMap>> GetMaps(short worldID, int tries = 3) {
            List<PsMap> maps;

            try {
                maps = await _MapCollection.GetZoneMaps(worldID, Zone.All);
            } catch (Exception ex) when (ex is CensusException || ex is CensusConnectionException || ex is CensusServiceUnavailableException) {
                _Logger.LogWarning($"Got timeout, trying {tries} more times");
                if (tries > 0) {
                    return await GetMaps(worldID, tries - 1);
                }
                throw;
            }

            return maps;
        }

    }
}
