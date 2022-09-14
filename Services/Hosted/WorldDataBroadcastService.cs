using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using watchtower.Code.Constants;
using watchtower.Code.Hubs;
using watchtower.Code.Hubs.Implementations;
using watchtower.Models;
using watchtower.Services.Repositories;

namespace watchtower.Services.Hosted {

    public class WorldDataBroadcastService : BackgroundService {

        private const string SERVICE_NAME = "worlddata_broadcast";

        private readonly ILogger<WorldDataBroadcastService> _Logger;
        private readonly IServiceHealthMonitor _ServiceHealthMonitor;

        private readonly WorldDataRepository _WorldDataRepository;

        private readonly IHubContext<WorldDataHub, IWorldDataHub> _DataHub;

        private List<int> _Durations = new() {
            60, 120
        };

        public WorldDataBroadcastService(ILogger<WorldDataBroadcastService> logger,
            IHubContext<WorldDataHub, IWorldDataHub> hub,
            WorldDataRepository worldDataRepo, IServiceHealthMonitor healthMon) {

            _Logger = logger;
            _ServiceHealthMonitor = healthMon ?? throw new ArgumentNullException(nameof(healthMon));

            _WorldDataRepository = worldDataRepo ?? throw new ArgumentNullException(nameof(worldDataRepo));

            _DataHub = hub ?? throw new ArgumentNullException(nameof(hub));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
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

                    foreach (int duration in _Durations) {
                        foreach (short worldID in World.PcStreams) {
                            try {
                                WorldData? data = _WorldDataRepository.Get(worldID, duration);
                                if (data == null) {
                                    continue;
                                }

                                string group = $"RealtimeData.{worldID}.";
                                if (duration == 60) {
                                    group += "Short";
                                } else if (duration == 120) {
                                    group += "Long";
                                } else {
                                    throw new Exception($"Unchecked duration {duration}");
                                }

                                _ = _DataHub.Clients.Group(group).UpdateData(data);

                                if ((data.Timestamp - TimeSpan.FromMinutes(3)) >= DateTime.UtcNow) {
                                    _Logger.LogWarning($"WorldData for {worldID} is out of date! Timestamp: {data.Timestamp}");
                                }
                            } catch (Exception ex) {
                                _Logger.LogError(ex, "Error updating clients listening on worldID {worldID}", worldID);
                            }
                        }
                    }

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
