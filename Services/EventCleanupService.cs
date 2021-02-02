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
        private const int _KeepPeriod = 60 * 60 * 1; // 60 seconds, 60 minutes, 1 hour

        private readonly ILogger<EventCleanupService> _Logger;

        public EventCleanupService(ILogger<EventCleanupService> logger) {
            _Logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            try {
                await Task.Delay(5000);

                while (!stoppingToken.IsCancellationRequested) {
                    Int64 currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    Int64 adjustedTime = currentTime - _KeepPeriod;

                    _Logger.LogInformation($"Clean up events beyond {_KeepPeriod} seconds");

                    Stopwatch time = Stopwatch.StartNew();

                    /*
                    lock (EventStore.Get().TrHeals) {
                        int pCount = EventStore.Get().TrHeals.Count;
                        EventStore.Get().TrHeals = EventStore.Get().TrHeals.Where(iter => iter.Timestamp >= adjustedTime).ToList();
                        _Logger.LogInformation($"Had {pCount} heals, have {EventStore.Get().TrHeals.Count} now");
                    }
                    lock (EventStore.Get().TrRevives) {
                        EventStore.Get().TrRevives = EventStore.Get().TrRevives.Where(iter => iter.Timestamp >= adjustedTime).ToList();
                    }
                    lock (EventStore.Get().TrResupplies) {
                        EventStore.Get().TrResupplies = EventStore.Get().TrResupplies.Where(iter => iter.Timestamp >= adjustedTime).ToList();
                    }

                    lock (EventStore.Get().NcHeals) {
                        EventStore.Get().NcHeals = EventStore.Get().NcHeals.Where(iter => iter.Timestamp >= adjustedTime).ToList();
                    }
                    lock (EventStore.Get().NcRevives) {
                        EventStore.Get().NcRevives = EventStore.Get().NcRevives.Where(iter => iter.Timestamp >= adjustedTime).ToList();
                    }
                    lock (EventStore.Get().NcResupplies) {
                        EventStore.Get().NcResupplies = EventStore.Get().NcResupplies.Where(iter => iter.Timestamp >= adjustedTime).ToList();
                    }

                    lock (EventStore.Get().VsHeals) {
                        EventStore.Get().VsHeals = EventStore.Get().VsHeals.Where(iter => iter.Timestamp >= adjustedTime).ToList();
                    }
                    lock (EventStore.Get().VsRevives) {
                        EventStore.Get().VsRevives = EventStore.Get().VsRevives.Where(iter => iter.Timestamp >= adjustedTime).ToList();
                    }
                    lock (EventStore.Get().VsResupplies) {
                        EventStore.Get().VsResupplies = EventStore.Get().VsResupplies.Where(iter => iter.Timestamp >= adjustedTime).ToList();
                    }
                    */

                    lock (CharacterStore.Get().Players) {
                        foreach (KeyValuePair<string, TrackedPlayer> entry in CharacterStore.Get().Players) {
                            entry.Value.Kills = entry.Value.Kills.Where(iter => iter >= adjustedTime).ToList();
                            entry.Value.Deaths = entry.Value.Deaths.Where(iter => iter >= adjustedTime).ToList();
                            entry.Value.Heals = entry.Value.Heals.Where(iter => iter >= adjustedTime).ToList();
                            entry.Value.Revives = entry.Value.Revives.Where(iter => iter >= adjustedTime).ToList();
                            entry.Value.Resupplies = entry.Value.Resupplies.Where(iter => iter >= adjustedTime).ToList();
                            entry.Value.Repairs = entry.Value.Repairs.Where(iter => iter >= adjustedTime).ToList();
                        }
                    }

                    time.Stop();
                    _Logger.LogInformation($"{DateTime.UtcNow} Took {time.ElapsedMilliseconds}ms to clean events");

                    await Task.Delay(_CleanupDelay * 1000);
                }
            } catch (Exception ex) {
                _Logger.LogError(ex, "EventCleanupService exception");
            }

            return;
        }

    }
}
