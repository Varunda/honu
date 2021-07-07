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
        private const int _SundyKeepPeriod = 60 * 5; // 60 seconds, 5 minutes
        private const int _AfkPeriod = 60 * 15; // 60 seconds, 15 minutes

        private readonly ILogger<EventCleanupService> _Logger;

        public EventCleanupService(ILogger<EventCleanupService> logger) { 
            _Logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            await Task.Delay(5000, stoppingToken);

            while (!stoppingToken.IsCancellationRequested) {
                try {
                    long currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    long adjustedTime = currentTime - (_KeepPeriod * 1000);
                    long sundyAdjustedTime = currentTime - (_SundyKeepPeriod * 1000);
                    long afkAdjustedTime = currentTime - (_AfkPeriod * 1000);

                    Stopwatch time = Stopwatch.StartNew();

                    lock (CharacterStore.Get().Players) {
                        foreach (KeyValuePair<string, TrackedPlayer> entry in CharacterStore.Get().Players) {
                            if (entry.Value.LatestEventTimestamp <= afkAdjustedTime && entry.Value.Online == true) {
                                //_Logger.LogDebug($"Setting {entry.Value.ID} to offline, latest event was at {entry.Value.LatestEventTimestamp}, needed {afkAdjustedTime}");
                                entry.Value.Online = false;
                            }

                            entry.Value.OnlineIntervals = entry.Value.OnlineIntervals.Where(iter => iter.End >= adjustedTime).ToList();
                        }
                    }

                    lock (NpcStore.Get().Npcs) {
                        List<string> toRemove = new List<string>();
                        foreach (KeyValuePair<string, TrackedNpc> entry in NpcStore.Get().Npcs) {
                            if (entry.Value.LatestEventAt < sundyAdjustedTime) {
                                // Don't want to delete keys while iterating, save the ones to delete
                                //_Logger.LogDebug($"NPC {entry.Value.NpcID}, latest: {entry.Value.LatestEventAt}, need: {sundyAdjustedTime}");
                                toRemove.Add(entry.Key);
                            }
                        }

                        foreach (string key in toRemove) {
                            NpcStore.Get().Npcs.TryRemove(key, out _);
                        }
                    }

                    //_Logger.LogTrace($"Removed {killsRemove} kills");

                    time.Stop();
                    _Logger.LogDebug($"{DateTime.UtcNow} Took {time.ElapsedMilliseconds}ms to clean events beyond {_KeepPeriod} seconds");

                    await Task.Delay(_CleanupDelay * 1000, stoppingToken);
                } catch (Exception) when (stoppingToken.IsCancellationRequested) {
                    _Logger.LogInformation($"Event cleanup service stopped");
                } catch (Exception ex) {
                    _Logger.LogError(ex, "EventCleanupService exception");
                }
            }
        }

    }
}
