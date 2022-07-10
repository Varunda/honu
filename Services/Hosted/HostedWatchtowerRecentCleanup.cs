using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Models;
using watchtower.Services.Db;

namespace watchtower.Services.Hosted {

    public class HostedWatchtowerRecentCleanup : BackgroundService {

        private readonly ILogger<HostedWatchtowerRecentCleanup> _Logger;
        private readonly IServiceHealthMonitor _ServiceHealthMonitor;
        private readonly IDbHelper _DbHelper;

        private const string SERVICE_NAME = "wt_cleanup";

        public HostedWatchtowerRecentCleanup(ILogger<HostedWatchtowerRecentCleanup> logger,
            IDbHelper dbHelper, IServiceHealthMonitor serviceHealthMonitor) {

            _Logger = logger;
            _DbHelper = dbHelper;
            _ServiceHealthMonitor = serviceHealthMonitor;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            _Logger.LogInformation($"{SERVICE_NAME}> starting");

            while (stoppingToken.IsCancellationRequested == false) {
                try {
                    ServiceHealthEntry health = _ServiceHealthMonitor.Get(SERVICE_NAME) ?? new ServiceHealthEntry() { Name = SERVICE_NAME };
                    if (health.Enabled == false) {
                        await Task.Delay(1000, stoppingToken);
                        continue;
                    }

                    Stopwatch timer = Stopwatch.StartNew();
                    Stopwatch total = Stopwatch.StartNew();

                    await using NpgsqlConnection conn = _DbHelper.Connection(task: "wt_cleanup", enlist: true);
                    long makeConn = timer.ElapsedMilliseconds; timer.Restart();

                    await conn.OpenAsync(stoppingToken);
                    long openConn = timer.ElapsedMilliseconds; timer.Restart();

                    await using NpgsqlBatch batch = new NpgsqlBatch(conn) {
                        BatchCommands = {
                            new NpgsqlBatchCommand() {
                                CommandText = @"DELETE FROM wt_recent_kills WHERE timestamp <= $1;",
                                Parameters = {
                                    new() { Value = DateTime.UtcNow - TimeSpan.FromHours(2) }
                                    //new() { Value = DateTime.UtcNow - TimeSpan.FromMinutes(1) }
                                }
                            }
                        }
                    };

                    long makeBatch = timer.ElapsedMilliseconds; timer.Restart();

                    await batch.PrepareAsync(stoppingToken);
                    long prepBatch = timer.ElapsedMilliseconds; timer.Restart();

                    await batch.ExecuteNonQueryAsync(stoppingToken);
                    long exec = timer.ElapsedMilliseconds; timer.Restart();

                    long ms = total.ElapsedMilliseconds;
                    long sum = makeConn + openConn + makeBatch + prepBatch + exec;

                    //if (ms > 100) {
                        _Logger.LogDebug($"{SERVICE_NAME}> took {ms}ms to run cleanup; "
                            + $"sum: {sum}ms, make conn: {makeConn}ms, open conn: {openConn}ms, make batch: {makeBatch}ms, prep batch: {prepBatch}ms, exec: {exec}ms");
                    //}

                    health.RunDuration = ms;
                    health.LastRan = DateTime.UtcNow;
                    health.Message = $"";
                    _ServiceHealthMonitor.Set(SERVICE_NAME, health);

                    await Task.Delay(1000 * 60, stoppingToken);
                } catch (Exception ex) {
                    _Logger.LogError(ex, $"error in watchtower cleanup");
                }
            }

            _Logger.LogInformation($"{SERVICE_NAME}> stopped");
        }

    }
}
