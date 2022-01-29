using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Code;
using watchtower.Models;
using watchtower.Services.Db;

namespace watchtower.Services {

    public class EventCleanupService : BackgroundService {

        private const string SERVICE_NAME = "event_cleanup";

        private const int _CleanupDelay = 15;
        private const int _KeepPeriod = 60 * 60 * 2; // 60 seconds, 60 minutes, 2 hours
        //private const int _KeepPeriod = 60 * 10 * 1; // 60 seconds, 10 minutes, 1 hours
        private const int _SundyKeepPeriod = 60 * 5; // 60 seconds, 5 minutes
        private const int _AfkPeriod = 60 * 15; // 60 seconds, 15 minutes
        private const int _ControlPeriod = 60; // 60 seconds
        private const int _VehicleDestroyPeriod = 60; // 60 seconds

        private readonly ILogger<EventCleanupService> _Logger;

        private readonly IServiceHealthMonitor _ServiceHealthMonitor;
        private readonly ISessionDbStore _SessionDb;

        public EventCleanupService(ILogger<EventCleanupService> logger,
            IServiceHealthMonitor healthMon, ISessionDbStore sessionDb) { 

            _Logger = logger;

            _ServiceHealthMonitor = healthMon ?? throw new ArgumentNullException(nameof(healthMon));
            _SessionDb = sessionDb ?? throw new ArgumentNullException(nameof(sessionDb));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            _Logger.LogInformation($"Started {SERVICE_NAME}");

            await Task.Delay(5000, stoppingToken);

            while (!stoppingToken.IsCancellationRequested) {
                try {
                    long currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    long adjustedTime = currentTime - (_KeepPeriod * 1000);
                    long sundyAdjustedTime = currentTime - (_SundyKeepPeriod * 1000);
                    long afkAdjustedTime = currentTime - (_AfkPeriod * 1000);
                    long controlAdjustedTime = currentTime - (_ControlPeriod * 1000);

                    Stopwatch time = Stopwatch.StartNew();

                    ServiceHealthEntry? healthEntry = _ServiceHealthMonitor.Get(SERVICE_NAME);
                    if (healthEntry == null) {
                        healthEntry = new ServiceHealthEntry() {
                            Name = SERVICE_NAME
                        };
                    }

                    lock (CharacterStore.Get().Players) {
                        foreach (KeyValuePair<string, TrackedPlayer> entry in CharacterStore.Get().Players) {
                            if (entry.Value.LatestEventTimestamp <= afkAdjustedTime && entry.Value.Online == true) {
                                //_Logger.LogDebug($"Setting {entry.Value.ID} to offline, latest event was at {entry.Value.LatestEventTimestamp}, needed {afkAdjustedTime}");
                                _ = _SessionDb.End(entry.Value, DateTime.UtcNow);
                            }
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

                    lock (PlayerFacilityControlStore.Get().Events) {
                        PlayerFacilityControlStore.Get().Events = PlayerFacilityControlStore.Get().Events.Where(iter => {
                            long timestamp = new DateTimeOffset(iter.Timestamp).ToUnixTimeMilliseconds();
                            return timestamp <= controlAdjustedTime;
                        }).ToList();
                    }

                    RecentSundererDestroyExpStore.Get().Clean(_VehicleDestroyPeriod);

                    time.Stop();

                    healthEntry.LastRan = DateTime.UtcNow;
                    healthEntry.RunDuration = time.ElapsedMilliseconds;
                    _ServiceHealthMonitor.Set(SERVICE_NAME, healthEntry);

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
