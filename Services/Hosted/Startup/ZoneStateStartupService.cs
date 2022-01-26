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

namespace watchtower.Services.Hosted.Startup {

    public class ZoneStateStartupService : IHostedService {

        private readonly ILogger<ZoneStateStartupService> _Logger;
        private readonly MapCollection _MapCollection;

        public ZoneStateStartupService(ILogger<ZoneStateStartupService> logger,
            MapCollection mapColl) {

            _Logger = logger;
            _MapCollection = mapColl;
        }

        public Task StartAsync(CancellationToken cancellationToken) {
            // Don't have this stop startup by blocking until it's done
            new Thread(async () => {
                Stopwatch timer = Stopwatch.StartNew();

                foreach (short worldID in World.All) {
                    _Logger.LogDebug($"Getting zone maps for {string.Join(", ", Zone.All.Select(iter => $"{Zone.GetName(iter)}/{iter}"))} for the world {World.GetName(worldID)}");
                    List<PsMap> maps = await _MapCollection.GetZoneMaps(worldID, Zone.All);

                    foreach (uint zoneID in Zone.All) {
                        List<PsMap> zoneMap = maps.Where(iter => iter.ZoneID == zoneID).ToList();
                        short? owner = _MapCollection.GetZoneMapOwner(worldID, zoneID, zoneMap);

                        lock (ZoneStateStore.Get().Zones) {
                            ZoneState state = ZoneStateStore.Get().GetZone(worldID, zoneID) ?? new() { WorldID = worldID, ZoneID = zoneID };

                            state.IsOpened = (owner == null);

                            ZoneStateStore.Get().SetZone(worldID, zoneID, state);
                        }
                    }
                }

                timer.Stop();
                _Logger.LogInformation($"Finished in {timer.ElapsedMilliseconds}ms");
            }).Start();

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) {
            return Task.CompletedTask;
        }

    }
}
