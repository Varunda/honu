using DaybreakGames.Census.Exceptions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Models;
using watchtower.Models.Census;
using watchtower.Models.Queues;
using watchtower.Services.Census;
using watchtower.Services.Db;
using watchtower.Services.Queues;

namespace watchtower.Services.Hosted {

    public class HostedBackgroundLogoutBufferQueue : BackgroundService {

        private readonly ILogger<HostedBackgroundLogoutBufferQueue> _Logger;
        private readonly BackgroundLogoutBufferQueue _Queue;
        private readonly IServiceHealthMonitor _ServiceHealthMonitor;

        private readonly ICharacterCollection _CharacterCensus;
        private readonly BackgroundCharacterWeaponStatQueue _WeaponQueue;
        private readonly LogoutBufferDbStore _LogoutDb;

        private const string SERVICE_NAME = "background_logout_buffer";
        private const int BATCH_SIZE = 50;
        private const int PERIOD_WAIT_LONG = 60;
        private const int PERIOD_WAIT_SHORT = 15;

        public HostedBackgroundLogoutBufferQueue(ILogger<HostedBackgroundLogoutBufferQueue> logger,
            BackgroundLogoutBufferQueue queue, ICharacterCollection charColl,
            BackgroundCharacterWeaponStatQueue weaponQueue, IServiceHealthMonitor healthMon,
            LogoutBufferDbStore logoutDb) {

            _Logger = logger;
            _Queue = queue ?? throw new ArgumentNullException(nameof(queue));
            _ServiceHealthMonitor = healthMon;
            _LogoutDb = logoutDb ?? throw new ArgumentNullException(nameof(logoutDb));

            _CharacterCensus = charColl;
            _WeaponQueue = weaponQueue;
        }

        protected async override Task ExecuteAsync(CancellationToken stoppingToken) {
            _Logger.LogInformation($"Started {SERVICE_NAME}");

            // Do an initial delay before the first run so there's some interesting data to actually look at
            await Task.Delay(5000, stoppingToken);

            while (stoppingToken.IsCancellationRequested == false) {
                try {
                    Stopwatch timer = Stopwatch.StartNew();

                    ServiceHealthEntry health = _ServiceHealthMonitor.Get(SERVICE_NAME) ?? new ServiceHealthEntry() { Name = SERVICE_NAME };
                    if (health.Enabled == false) {
                        await Task.Delay(1000, stoppingToken);
                        continue;
                    }

                    List<LogoutBufferEntry> entries = await _LogoutDb.GetPending(stoppingToken);
                    long dbTime = timer.ElapsedMilliseconds;
                    timer.Restart();

                    List<PsCharacter> chars = new List<PsCharacter>(entries.Count);

                    try {
                        chars = await _CharacterCensus.GetByIDs(entries.Select(i => i.CharacterID).ToList());
                    } catch (Exception ex) {
                        _Logger.LogError(ex, "Failed to get characters for logout buffer, trying again in a bit");
                        await Task.Delay(1000 * PERIOD_WAIT_SHORT);
                        continue;
                    }

                    long censusTime = timer.ElapsedMilliseconds;
                    timer.Restart();

                    int requeued = 0;
                    int left = 0;
                    int notFound = 0;

                    foreach (LogoutBufferEntry entry in entries) {
                        PsCharacter? c = chars.FirstOrDefault(iter => iter.ID == entry.CharacterID);
                        // Sometimes characters are deleted before Census updates them
                        if (c == null) {
                            ++notFound;
                            ++entry.NotFoundCount;
                            if (entry.NotFoundCount < 20) {
                                await _LogoutDb.Upsert(entry, stoppingToken);
                            } else {
                                _Logger.LogWarning($"Missing character {entry.CharacterID} after getting batch. Discarding");
                                await _LogoutDb.Delete(entry.CharacterID, stoppingToken);
                            }
                        } else {
                            TimeSpan diff = entry.LoginTime - c.DateLastLogin;
                            //_Logger.LogDebug($"Character {c.Name} logged in from Honu at {entry.LoginTime:u}, Census says {c.DateLastLogin}, difference of {diff}");

                            if (diff > TimeSpan.FromMinutes(5)) {
                                //_Logger.LogDebug($"Character {c.Name} has {diff} time span between, putting back into queue");
                                ++requeued;
                                await _LogoutDb.Upsert(entry, stoppingToken);
                            } else {
                                _Logger.LogDebug($"Character {c.Name}/{c.WorldID} has {diff} time span, Census is updated, putting into weapon queue. Took {-(entry.Timestamp - DateTime.UtcNow)} to update");
                                ++left;
                                await _LogoutDb.Delete(entry.CharacterID, stoppingToken);
                                _WeaponQueue.Queue(entry.CharacterID);
                            }
                        }
                    }

                    long processTime = timer.ElapsedMilliseconds;
                    long totalTime = dbTime + censusTime + processTime;

                    _Logger.LogDebug($"Took {totalTime}ms to run for {entries.Count} entries. DB took {dbTime}ms, Census took {censusTime}ms, processing took {processTime}ms."
                        + $" {requeued} stayed, {left} removed, {notFound} were missing");

                    health.RunDuration = totalTime;
                    health.LastRan = DateTime.UtcNow;
                    _ServiceHealthMonitor.Set(SERVICE_NAME, health);
                } catch (Exception) when (stoppingToken.IsCancellationRequested == true) {
                    _Logger.LogInformation($"Stopping {SERVICE_NAME}, DB will persist the data");
                } catch (Exception ex) {
                    _Logger.LogError(ex, $"Error in {SERVICE_NAME}");
                } finally {
                    await Task.Delay(1000 * PERIOD_WAIT_LONG, stoppingToken);
                }
            }

        }

    }
}
