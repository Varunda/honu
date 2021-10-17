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
using watchtower.Services.Census;

namespace watchtower.Services.Hosted.Startup {

    public class ZoneStateStartupService : IHostedService {

        private readonly ILogger<ZoneStateStartupService> _Logger;
        private readonly IMapCollection _MapCollection;

        public ZoneStateStartupService(ILogger<ZoneStateStartupService> logger,
            IMapCollection mapColl) {

            _Logger = logger;
            _MapCollection = mapColl;
        }

        public Task StartAsync(CancellationToken cancellationToken) {
            // Don't have this stop startup by blocking until it's done
            new Thread(async () => {
                Stopwatch timer = Stopwatch.StartNew();

                foreach (short worldID in World.All) {
                    foreach (uint zoneID in Zone.All) {
                        short? owner = await _MapCollection.GetZoneMapOwner(worldID, zoneID);

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
