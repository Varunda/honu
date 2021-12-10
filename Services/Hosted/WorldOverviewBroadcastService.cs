using Microsoft.AspNetCore.SignalR;
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
using watchtower.Code.Hubs;
using watchtower.Code.Hubs.Implementations;
using watchtower.Constants;
using watchtower.Models;

namespace watchtower.Services.Hosted {

    public class WorldOverviewBroadcastService : BackgroundService {

        private const string SERVICE_NAME = "worldoverview_broadcast";

        private readonly ILogger<WorldOverviewBroadcastService> _Logger;
        private readonly IServiceHealthMonitor _ServiceHealthMonitor;

        private readonly IHubContext<WorldOverviewHub, IWorldOverviewHub> _DataHub;

        public WorldOverviewBroadcastService(ILogger<WorldOverviewBroadcastService> logger,
            IServiceHealthMonitor healthMon, IHubContext<WorldOverviewHub, IWorldOverviewHub> hub) {

            _Logger = logger;

            _ServiceHealthMonitor = healthMon;
            _DataHub = hub;
        }

        protected async override Task ExecuteAsync(CancellationToken stoppingToken) {
            _Logger.LogInformation($"Started {SERVICE_NAME}");

            while (stoppingToken.IsCancellationRequested == false) {
                try {
                    Stopwatch time = Stopwatch.StartNew();

                    ServiceHealthEntry? healthEntry = _ServiceHealthMonitor.Get(SERVICE_NAME);
                    if (healthEntry == null) {
                        healthEntry = new ServiceHealthEntry() {
                            Name = SERVICE_NAME
                        };
                    }

                    WorldOverview connery = new() { WorldID = World.Connery, WorldName = "Connery" };
                    WorldOverview cobalt = new() { WorldID = World.Cobalt, WorldName = "Cobalt" };
                    WorldOverview jaeger = new() { WorldID = World.Jaeger, WorldName = "Jaeger" };
                    WorldOverview miller = new() { WorldID = World.Miller, WorldName = "Miller" };
                    WorldOverview emerald = new() { WorldID = World.Emerald, WorldName = "Emerald" };
                    WorldOverview soltech = new() { WorldID = World.SolTech, WorldName = "SolTech" };

                    List<WorldOverview> worlds = new List<WorldOverview>() {
                        connery, cobalt, jaeger, miller, emerald, soltech
                    };

                    lock (CharacterStore.Get().Players) {
                        foreach (KeyValuePair<string, TrackedPlayer> iter in CharacterStore.Get().Players) {
                            TrackedPlayer c = iter.Value;

                            if (c.Online == false) {
                                continue;
                            }

                            if (c.WorldID == World.Connery) {
                                ++connery.PlayersOnline;
                            } else if (c.WorldID == World.Cobalt) {
                                ++cobalt.PlayersOnline;
                            } else if (c.WorldID == World.Emerald) {
                                ++emerald.PlayersOnline;
                            } else if (c.WorldID == World.Jaeger) {
                                ++jaeger.PlayersOnline;
                            } else if (c.WorldID == World.Miller) {
                                ++miller.PlayersOnline;
                            } else if (c.WorldID == World.SolTech) {
                                ++soltech.PlayersOnline;
                            }
                        }
                    }

                    foreach (WorldOverview world in worlds) {
                        lock (ZoneStateStore.Get().Zones) {
                            ZoneState? indar = ZoneStateStore.Get().GetZone(world.WorldID, Zone.Indar);
                            ZoneState? hossin = ZoneStateStore.Get().GetZone(world.WorldID, Zone.Hossin);
                            ZoneState? amerish = ZoneStateStore.Get().GetZone(world.WorldID, Zone.Amerish);
                            ZoneState? esamir = ZoneStateStore.Get().GetZone(world.WorldID, Zone.Esamir);

                            if (indar != null) { world.Zones.Add(indar); }
                            if (hossin != null) { world.Zones.Add(hossin); }
                            if (amerish != null) { world.Zones.Add(amerish); }
                            if (esamir != null) { world.Zones.Add(esamir); }
                        }
                    }

                    await _DataHub.Clients.All.UpdateData(worlds);

                    healthEntry.RunDuration = time.ElapsedMilliseconds;
                    healthEntry.LastRan = DateTime.UtcNow;
                    _ServiceHealthMonitor.Set(SERVICE_NAME, healthEntry);

                    await Task.Delay(1000 * 5, stoppingToken);
                } catch (Exception ex) when (stoppingToken.IsCancellationRequested == false) {
                    _Logger.LogError(ex, "Failed to update connected clients");
                } catch (Exception) when (stoppingToken.IsCancellationRequested == true) {
                    _Logger.LogInformation($"Stopped {SERVICE_NAME}");
                }
            }
        }

    }
}
