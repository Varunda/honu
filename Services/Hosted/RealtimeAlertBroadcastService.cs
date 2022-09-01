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
using watchtower.Models.RealtimeAlert;
using watchtower.Models.Watchtower;
using watchtower.Services.Repositories;

namespace watchtower.Services.Hosted {

    public class RealtimeAlertBroadcastServer : BackgroundService {

        private const string SERVICE_NAME = "network_broadcast";

        private readonly ILogger<RealtimeAlertBroadcastServer> _Logger;
        private readonly IServiceHealthMonitor _ServiceHealthMonitor;

        private readonly RealtimeAlertRepository _RealtimeAlertRepository;

        private readonly IHubContext<RealtimeAlertHub, IRealtimeAlertHub> _AlertHub;

        public RealtimeAlertBroadcastServer(ILogger<RealtimeAlertBroadcastServer> logger,
            IServiceHealthMonitor healthMon, IHubContext<RealtimeAlertHub, IRealtimeAlertHub> hub,
            RealtimeAlertRepository realtimeAlertRepository) {

            _Logger = logger;
            _ServiceHealthMonitor = healthMon;

            _AlertHub = hub;
            _RealtimeAlertRepository = realtimeAlertRepository;
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

                    foreach (RealtimeAlert a in _RealtimeAlertRepository.GetAll()) {
                        string group = $"RealtimeAlert.{a.WorldID}.{a.ZoneID}";
                        RealtimeAlert alert = new(a.VS, a.NC, a.TR);
                        alert.WorldID = a.WorldID;
                        alert.ZoneID = a.ZoneID;
                        alert.Timestamp = a.Timestamp;
                        await _AlertHub.Clients.Group(group).UpdateAlert(alert);
                    }

                    healthEntry.RunDuration = time.ElapsedMilliseconds;
                    healthEntry.LastRan = DateTime.UtcNow;
                    _ServiceHealthMonitor.Set(SERVICE_NAME, healthEntry);

                    await Task.Delay(1000 * 1, stoppingToken);
                } catch (Exception ex) when (stoppingToken.IsCancellationRequested == false) {
                    _Logger.LogError(ex, "Failed to update connected clients");
                } catch (Exception) when (stoppingToken.IsCancellationRequested == true) {
                    _Logger.LogInformation($"Stopped {SERVICE_NAME}");
                }
            }
        }

    }
}
