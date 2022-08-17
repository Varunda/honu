using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Models;
using watchtower.Services.Db;

namespace watchtower.Services.Hosted {

    public class HostedBackgroundWeaponStatSnapshotCreator : BackgroundService {

        private readonly ILogger<HostedBackgroundWeaponStatSnapshotCreator> _Logger;

        private readonly WeaponStatSnapshotDbStore _SnapshotDb;
        private readonly IServiceHealthMonitor _ServiceHealthMonitor;

        private const int WHEN_HOUR = 17;
        private const string SERVICE_NAME = "background_weapon_snapshot";

        public HostedBackgroundWeaponStatSnapshotCreator(ILogger<HostedBackgroundWeaponStatSnapshotCreator> logger,
            WeaponStatSnapshotDbStore snapshotDb, IServiceHealthMonitor serviceHealthMonitor) {

            _Logger = logger;
            _SnapshotDb = snapshotDb;
            _ServiceHealthMonitor = serviceHealthMonitor;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            _Logger.LogDebug($"{SERVICE_NAME}> Started");

            while (stoppingToken.IsCancellationRequested == false) {
                try {
                    Stopwatch timer = Stopwatch.StartNew();

                    ServiceHealthEntry health = _ServiceHealthMonitor.Get(SERVICE_NAME) ?? new ServiceHealthEntry() { Name = SERVICE_NAME };
                    if (health.Enabled == false) {
                        await Task.Delay(1000, stoppingToken);
                        continue;
                    }

                    DateTime now = DateTime.UtcNow;
                    if (now.Hour != WHEN_HOUR || now.Minute != 5) {
                        //_Logger.LogTrace($"{SERVICE_NAME}> It's currently {now:u}, must be {WHEN_HOUR}:00");
                        await Task.Delay(1000 * 30, stoppingToken);
                        continue;
                    }

                    DateTime? lastRan = await _SnapshotDb.GetMostRecent();
                    _Logger.LogInformation($"{SERVICE_NAME}> Last ran at {lastRan?.ToString("u")}");

                    await _SnapshotDb.Generate(stoppingToken);

                    health.RunDuration = timer.ElapsedMilliseconds;
                    health.LastRan = DateTime.UtcNow;
                    health.Message = $"ran at {now:u}";
                } catch (Exception) when (stoppingToken.IsCancellationRequested == true) {
                    _Logger.LogInformation($"{SERVICE_NAME}> Stopping");
                } catch (Exception ex) when (stoppingToken.IsCancellationRequested == false) {
                    _Logger.LogError(ex, $"error when generating weapon stat snapshot");
                }
            }

            _Logger.LogDebug($"Stopped");
        }

    }
}
