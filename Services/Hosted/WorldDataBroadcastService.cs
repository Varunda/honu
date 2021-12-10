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
using watchtower.Code.Hubs;
using watchtower.Code.Hubs.Implementations;
using watchtower.Models;
using watchtower.Services.Repositories;

namespace watchtower.Services.Hosted {

    public class WorldDataBroadcastService : BackgroundService {

        private const string SERVICE_NAME = "worlddata_broadcast";

        private readonly ILogger<WorldDataBroadcastService> _Logger;
        private readonly IServiceHealthMonitor _ServiceHealthMonitor;

        private readonly IWorldDataRepository _WorldDataRepository;

        private readonly IHubContext<WorldDataHub, IWorldDataHub> _DataHub;

        private List<short> _WorldIDs = new List<short>() {
            1, 10, 13, 17, 19, 40
        };

        public WorldDataBroadcastService(ILogger<WorldDataBroadcastService> logger,
            IHubContext<WorldDataHub, IWorldDataHub> hub,
            IWorldDataRepository worldDataRepo, IServiceHealthMonitor healthMon) {

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

                    foreach (short worldID in _WorldIDs) {
                        try {
                            WorldData? data = _WorldDataRepository.Get(worldID);
                            if (data != null) {
                                _ = _DataHub.Clients.Group(worldID.ToString()).UpdateData(data);

                                if ((data.Timestamp - TimeSpan.FromMinutes(3)) >= DateTime.UtcNow) {
                                    _Logger.LogWarning($"WorldData for {worldID} is out of date! Timestamp: {data.Timestamp}");
                                }
                            } else {
                                //_Logger.LogWarning($"Missing world data for {worldID}");
                            }
                        } catch (Exception ex) {
                            _Logger.LogError(ex, "Error updating clients listening on worldID {worldID}", worldID);
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
