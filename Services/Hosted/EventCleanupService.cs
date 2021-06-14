using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Models;

namespace watchtower.Services {

    public class EventCleanupService : BackgroundService {

        private const int _CleanupDelay = 15;
        private const int _KeepPeriod = 60 * 60 * 2; // 60 seconds, 60 minutes, 2 hours

        private readonly ILogger<EventCleanupService> _Logger;

        public EventCleanupService(ILogger<EventCleanupService> logger) {
            _Logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            try {
                await Task.Delay(5000, stoppingToken);

                while (!stoppingToken.IsCancellationRequested) {
                    long currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    long adjustedTime = currentTime - _KeepPeriod;

                    Stopwatch time = Stopwatch.StartNew();

                    int killsRemove = 0;

                    lock (CharacterStore.Get().Players) {
                        foreach (KeyValuePair<string, TrackedPlayer> entry in CharacterStore.Get().Players) {
                            int pk = entry.Value.Kills.Count;
                            entry.Value.Kills = entry.Value.Kills.Where(iter => iter >= adjustedTime).ToList();
                            killsRemove += pk - entry.Value.Kills.Count;
                            entry.Value.Deaths = entry.Value.Deaths.Where(iter => iter >= adjustedTime).ToList();
                            entry.Value.Heals = entry.Value.Heals.Where(iter => iter >= adjustedTime).ToList();
                            entry.Value.Revives = entry.Value.Revives.Where(iter => iter >= adjustedTime).ToList();
                            entry.Value.Resupplies = entry.Value.Resupplies.Where(iter => iter >= adjustedTime).ToList();
                            entry.Value.Repairs = entry.Value.Repairs.Where(iter => iter >= adjustedTime).ToList();
                            entry.Value.Spawns = entry.Value.Spawns.Where(iter => iter >= adjustedTime).ToList();
                            entry.Value.Assists = entry.Value.Assists.Where(iter => iter >= adjustedTime).ToList();
                        }
                    }

                    _Logger.LogTrace($"Removed {killsRemove} kills");

                    time.Stop();
                    _Logger.LogDebug($"{DateTime.UtcNow} Took {time.ElapsedMilliseconds}ms to clean events beyond {_KeepPeriod} seconds");

                    await Task.Delay(_CleanupDelay * 1000, stoppingToken);
                }
            } catch (Exception ex) {
                _Logger.LogError(ex, "EventCleanupService exception");
            }

            return;
        }

    }
}
