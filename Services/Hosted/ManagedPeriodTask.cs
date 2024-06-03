using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Models;
using watchtower.Services.Implementations;

namespace watchtower.Services.Hosted {

    public abstract class ManagedPeriodTask : BackgroundService {

        private readonly ILogger _Logger;
        private readonly IServiceHealthMonitor _HealthMonitor;

        private readonly string _ServiceName = "";
        private readonly TimeSpan _TimeBetweenRuns;

        private DateTime _LastDisabledReminder = DateTime.MinValue;

        public ManagedPeriodTask(string serviceName, TimeSpan delayBetweenRuns,
            ILogger logger, IServiceHealthMonitor healthMonitor) {

            _ServiceName = serviceName;
            _TimeBetweenRuns = delayBetweenRuns;

            _Logger = logger;
            _HealthMonitor = healthMonitor;

            ServiceHealthEntry? healthEntry = _HealthMonitor.Get(_ServiceName);
            if (healthEntry != null) {
                throw new Exception($"duplicate {nameof(ManagedPeriodTask)} configured, service with name of '{_ServiceName}' already exists!");
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            while (stoppingToken.IsCancellationRequested == false) {
                ServiceHealthEntry healthEntry = _HealthMonitor.GetOrCreate(_ServiceName);
                if (healthEntry.Enabled == false) {
                    if (DateTime.UtcNow > _LastDisabledReminder) {
                        _LastDisabledReminder = DateTime.UtcNow + TimeSpan.FromMinutes(5);
                        _Logger.LogInformation($"disabled service reminder [ServiceName={_ServiceName}] [nextReminder={_LastDisabledReminder}]");
                    }

                    await Task.Delay(1000, stoppingToken);
                    continue;
                }

                DateTime start = DateTime.UtcNow;
                try {
                    await Run(stoppingToken);
                } catch (Exception ex) {
                    _Logger.LogError(ex, $"failed to run service [ServiceName={_ServiceName}]");
                }
                DateTime end = DateTime.UtcNow;

                healthEntry.RunDuration = (int)(end - start).TotalMilliseconds;
                healthEntry.LastRan = DateTime.UtcNow;

                TimeSpan stopDelay = _TimeBetweenRuns - (end - start);
                if (stopDelay > TimeSpan.Zero) {
                    await Task.Delay(stopDelay, stoppingToken);
                }
            }
        }

        protected abstract Task Run(CancellationToken cancel);

    }
}
