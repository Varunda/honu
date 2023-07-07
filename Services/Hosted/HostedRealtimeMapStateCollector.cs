using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Models;
using watchtower.Models.Db;
using watchtower.Services.Census;
using watchtower.Services.Db;
using watchtower.Services.Repositories;

namespace watchtower.Services.Hosted {

    public class HostedRealtimeMapStateCollector : BackgroundService {

        private readonly ILogger<HostedRealtimeMapStateCollector> _Logger;
        private readonly IServiceHealthMonitor _ServiceHealth;

        private readonly RealtimeMapStateRepository _RealtimeMapRepository;

        private const string SERVICE_NAME = "realtime_map_state_collector";

        private readonly Dictionary<string, RealtimeMapState> _PreviousState = new();

        public HostedRealtimeMapStateCollector(ILogger<HostedRealtimeMapStateCollector> logger,
            IServiceHealthMonitor serviceHealth, RealtimeMapStateRepository realtimeMapRepository) {

            _Logger = logger;
            _ServiceHealth = serviceHealth;

            _RealtimeMapRepository = realtimeMapRepository;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            _Logger.LogInformation($"starting");

            while (stoppingToken.IsCancellationRequested == false) {
                try {
                    ServiceHealthEntry health = _ServiceHealth.Get(SERVICE_NAME) ?? new() {
                        Name = SERVICE_NAME,
                    };

                    if (health.Enabled == false) {
                        await Task.Delay(1000, stoppingToken);
                        continue;
                    }

                    Stopwatch timer = Stopwatch.StartNew();

                    await _RealtimeMapRepository.Update(stoppingToken);

                    health.LastRan = DateTime.UtcNow;
                    health.RunDuration = timer.ElapsedMilliseconds;
                    _ServiceHealth.Set(SERVICE_NAME, health);

                } catch (Exception ex) when (stoppingToken.IsCancellationRequested == false) {
                    _Logger.LogError(ex, $"failed to get realtime map state");
                } catch (Exception) when (stoppingToken.IsCancellationRequested == true) {
                    _Logger.LogInformation($"stopping service");
                }

                await Task.Delay(60 * 1000, stoppingToken);
            }
        }

    }
}
