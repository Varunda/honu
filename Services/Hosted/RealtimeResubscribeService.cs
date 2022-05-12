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
using watchtower.Realtime;
using watchtower.Services.Repositories;

namespace watchtower.Services.Hosted {

    public class RealtimeResubcribeService : BackgroundService {

        private const string SERVICE_NAME = "realtime_resubscribe";

        private readonly ILogger<RealtimeResubcribeService> _Logger;

        private readonly IRealtimeMonitor _RealtimeMonitor;
        private readonly IServiceHealthMonitor _ServiceHealthMonitor;

        public RealtimeResubcribeService(ILogger<RealtimeResubcribeService> logger,
            IRealtimeMonitor realtimeMonitor, IServiceHealthMonitor healthMon) {

            _Logger = logger;

            _RealtimeMonitor = realtimeMonitor ?? throw new ArgumentNullException(nameof(realtimeMonitor));
            _ServiceHealthMonitor = healthMon ?? throw new ArgumentNullException(nameof(healthMon));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            _Logger.LogInformation($"Started {SERVICE_NAME}");

            while (stoppingToken.IsCancellationRequested == false) {
                try {
                    ServiceHealthEntry? healthEntry = _ServiceHealthMonitor.Get(SERVICE_NAME);
                    if (healthEntry == null) {
                        healthEntry = new ServiceHealthEntry() {
                            Name = SERVICE_NAME
                        };
                    }

                    healthEntry.LastRan = DateTime.UtcNow;

                    Stopwatch timer = Stopwatch.StartNew();

                    _Logger.LogInformation($"{SERVICE_NAME}> Resubscribing census subscriptions");

                    await _RealtimeMonitor.Reconnect();
                    await _RealtimeMonitor.Resubscribe();

                    healthEntry.RunDuration = timer.ElapsedMilliseconds;
                    _ServiceHealthMonitor.Set(SERVICE_NAME, healthEntry);

                    await Task.Delay(1000 * 60 * 10, stoppingToken);
                } catch (Exception ex) when (stoppingToken.IsCancellationRequested == false) {
                    _Logger.LogError(ex, $"Error in {SERVICE_NAME}");
                } catch (Exception) when (stoppingToken.IsCancellationRequested == true) {
                    _Logger.LogInformation($"Stopping {SERVICE_NAME}");
                }
            }

        }

    }
}
