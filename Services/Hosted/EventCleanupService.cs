using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Code;
using watchtower.Code.Tracking;
using watchtower.Models;
using watchtower.Models.Queues;
using watchtower.Realtime;
using watchtower.Services.Db;
using watchtower.Services.Queues;
using watchtower.Services.Repositories;

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
        private readonly SessionRepository _SessionRepository;
        private readonly IEventHandler _EventHandler;
        private readonly SessionEndQueue _SessionEndQueue;

        public EventCleanupService(ILogger<EventCleanupService> logger,
            IServiceHealthMonitor healthMon, SessionRepository sessionRepository,
            IEventHandler eventHandler, SessionEndQueue sessionEndQueue) {

            _Logger = logger;

            _ServiceHealthMonitor = healthMon ?? throw new ArgumentNullException(nameof(healthMon));
            _SessionRepository = sessionRepository;
            _EventHandler = eventHandler;
            _SessionEndQueue = sessionEndQueue;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            _Logger.LogInformation($"Started {SERVICE_NAME}");

            await Task.Delay(5000, stoppingToken);

            while (!stoppingToken.IsCancellationRequested) {
                try {
                    using Activity? rootTrace = HonuActivitySource.Root.StartActivity("periodic cleanup - start");
                    DateTime now = _EventHandler.MostRecentProcess();
                    DateTimeOffset nowOffset = new(now);
                    long nowTime = nowOffset.ToUnixTimeMilliseconds();
                    //long currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    long sundyAdjustedTime = nowTime - (_SundyKeepPeriod * 1000);
                    long afkAdjustedTime = nowTime - (_AfkPeriod * 1000);
                    long controlAdjustedTime = nowTime - (_ControlPeriod * 1000);

                    rootTrace?.AddTag("honu.now", $"{DateTime.UtcNow:u}");
                    rootTrace?.AddTag("honu.event-time-date", $"{now:u}");
                    rootTrace?.AddTag("honu.event-time", nowTime);
                    rootTrace?.AddTag("honu.sundy-time", sundyAdjustedTime);
                    rootTrace?.AddTag("honu.afk-time", afkAdjustedTime);
                    rootTrace?.AddTag("honu.control-time", controlAdjustedTime);

                    Stopwatch time = Stopwatch.StartNew();

                    ServiceHealthEntry? healthEntry = _ServiceHealthMonitor.Get(SERVICE_NAME);
                    if (healthEntry == null) {
                        healthEntry = new ServiceHealthEntry() {
                            Name = SERVICE_NAME
                        };
                    }

                    // character cleanup
                    using (Activity? trace = HonuActivitySource.Root.StartActivity("periodic cleanup - characters")) {
                        int count = 0;
                        int total = 0;
                        lock (CharacterStore.Get().Players) {
                            foreach (KeyValuePair<string, TrackedPlayer> entry in CharacterStore.Get().Players) {
                                ++total;
                                if (entry.Value.LatestEventTimestamp <= afkAdjustedTime && entry.Value.Online == true) {
                                    //_Logger.LogDebug($"Setting {entry.Value.ID} to offline, latest event was at {entry.Value.LatestEventTimestamp}, needed {afkAdjustedTime}");
                                    //_ = _SessionRepository.End(entry.Value.ID, DateTime.UtcNow);
                                    _SessionEndQueue.Queue(new SessionEndQueueEntry() {
                                        CharacterID = entry.Value.ID,
                                        Timestamp = now,
                                        SessionID = entry.Value.SessionID
                                    });
                                    ++count;
                                }
                            }
                        }
                        trace?.AddTag("honu.total", total);
                        trace?.AddTag("honu.count", count);
                    }

                    // sundy cleanup
                    using (Activity? trace = HonuActivitySource.Root.StartActivity("periodic cleanup - sundies")) {
                        int count = 0;
                        int total = 0;
                        lock (NpcStore.Get().Npcs) {
                            List<string> toRemove = new List<string>();
                            foreach (KeyValuePair<string, TrackedNpc> entry in NpcStore.Get().Npcs) {
                                ++total;
                                if (entry.Value.LatestEventAt < sundyAdjustedTime) {
                                    // Don't want to delete keys while iterating, save the ones to delete
                                    //_Logger.LogDebug($"NPC {entry.Value.NpcID}, latest: {entry.Value.LatestEventAt}, need: {sundyAdjustedTime}");
                                    toRemove.Add(entry.Key);
                                }
                            }

                            foreach (string key in toRemove) {
                                ++count;
                                NpcStore.Get().Npcs.TryRemove(key, out _);
                            }
                        }
                        trace?.AddTag("honu.total", total);
                        trace?.AddTag("honu.count", count);
                    }

                    // player control cleanup 
                    using (Activity? trace = HonuActivitySource.Root.StartActivity("periodic cleanup - player control")) {
                        int count = 0;
                        int before = 0;
                        lock (PlayerFacilityControlStore.Get().Events) {
                            before = PlayerFacilityControlStore.Get().Events.Count;
                            PlayerFacilityControlStore.Get().Events = PlayerFacilityControlStore.Get().Events.Where(iter => {
                                long timestamp = new DateTimeOffset(iter.Timestamp).ToUnixTimeMilliseconds();
                                return timestamp >= controlAdjustedTime;
                            }).ToList();
                            int after = PlayerFacilityControlStore.Get().Events.Count;
                            count = before - after;
                        }
                        trace?.AddTag("honu.total", before);
                        trace?.AddTag("honu.count", count);
                    }

                    // sundy destroy cleanup
                    using (Activity? trace = HonuActivitySource.Root.StartActivity("periodic cleanup - sundy destroy")) {
                        DateTime removeBefore = now - TimeSpan.FromSeconds(_ControlPeriod);
                        int count = RecentSundererDestroyExpStore.Get().Clean(removeBefore);
                        trace?.AddTag("honu.count", count);
                    }

                    time.Stop();

                    healthEntry.LastRan = DateTime.UtcNow;
                    healthEntry.RunDuration = time.ElapsedMilliseconds;
                    _ServiceHealthMonitor.Set(SERVICE_NAME, healthEntry);
                    rootTrace?.Stop();

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
