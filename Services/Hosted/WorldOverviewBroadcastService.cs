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
using watchtower.Services.Repositories;

namespace watchtower.Services.Hosted {

    public class WorldOverviewBroadcastService : BackgroundService {

        private const string SERVICE_NAME = "worldoverview_broadcast";

        private readonly ILogger<WorldOverviewBroadcastService> _Logger;
        private readonly IServiceHealthMonitor _ServiceHealthMonitor;

        private readonly IHubContext<WorldOverviewHub, IWorldOverviewHub> _DataHub;

        private readonly WorldOverviewRepository _WorldOverviewRepository;

        public WorldOverviewBroadcastService(ILogger<WorldOverviewBroadcastService> logger,
            IServiceHealthMonitor healthMon, IHubContext<WorldOverviewHub, IWorldOverviewHub> hub,
            WorldOverviewRepository worldRepo) {

            _Logger = logger;

            _ServiceHealthMonitor = healthMon;
            _DataHub = hub;
            _WorldOverviewRepository = worldRepo;
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

                    List<WorldOverview> worlds = _WorldOverviewRepository.Build();

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
