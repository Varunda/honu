﻿using Microsoft.AspNetCore.SignalR;
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
using watchtower.Models.Watchtower;
using watchtower.Services.Repositories;

namespace watchtower.Services.Hosted {

    public class RealtimeNetworkBroadcastService : BackgroundService {

        private const string SERVICE_NAME = "network_broadcast";

        private readonly ILogger<RealtimeNetworkBroadcastService> _Logger;
        private readonly IServiceHealthMonitor _ServiceHealthMonitor;

        private readonly RealtimeNetworkRepository _NetworkRepository;

        private readonly IHubContext<RealtimeNetworkHub, IRealtimeNetworkHub> _NetworkHub;

        public RealtimeNetworkBroadcastService(ILogger<RealtimeNetworkBroadcastService> logger,
            IServiceHealthMonitor healthMon, RealtimeNetworkRepository networkRepository,
            IHubContext<RealtimeNetworkHub, IRealtimeNetworkHub> networkHub) {

            _Logger = logger;
            _ServiceHealthMonitor = healthMon ?? throw new ArgumentNullException(nameof(healthMon));

            _NetworkRepository = networkRepository;
            _NetworkHub = networkHub;
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

                    foreach (short worldID in World.All) {
                        try {
                            RealtimeNetwork? network = _NetworkRepository.Get(worldID);
                            if (network != null) {
                                await _NetworkHub.Clients.Group($"RealtimeNetwork.{worldID}").UpdateNetwork(network);
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
