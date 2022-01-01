using DaybreakGames.Census.Exceptions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
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

    /// <summary>
    ///     Background buffer that will buffer logouts until Census has updated their data.
    ///     This prevents Honu from attempting to do a character update before Census 
    ///         has updated the data
    /// </summary>
    public class HostedBackgroundLogoutBuffer : BackgroundService {

        private readonly ILogger<HostedBackgroundLogoutBuffer> _Logger;
        private readonly IServiceHealthMonitor _ServiceHealthMonitor;

        private readonly ICharacterCollection _CharacterCensus;
        private readonly BackgroundCharacterWeaponStatQueue _WeaponQueue;
        private readonly LogoutBufferDbStore _LogoutDb;

        private const string SERVICE_NAME = "logout_buffer";
        private const int PERIOD_WAIT_LONG = 60;
        private const int PERIOD_WAIT_SHORT = 15;
        private const int TASK_COUNT = 4;

        public HostedBackgroundLogoutBuffer(ILogger<HostedBackgroundLogoutBuffer> logger,
            ICharacterCollection charColl, BackgroundCharacterWeaponStatQueue weaponQueue,
            IServiceHealthMonitor healthMon, LogoutBufferDbStore logoutDb) {

            _Logger = logger;
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

                    List<LogoutBufferEntry> entries;
                    try {
                        entries = await _LogoutDb.GetPending(stoppingToken);
                    } catch (Exception ex) {
                        _Logger.LogError(ex, $"failed to get pending entries in logout buffer");
                        await Task.Delay(5000, stoppingToken);
                        continue;
                    }
                    long dbTime = timer.ElapsedMilliseconds;
                    timer.Restart();

                    List<PsCharacter> chars = new List<PsCharacter>(entries.Count);

                    try {
                        chars = await _CharacterCensus.GetByIDs(entries.Select(i => i.CharacterID).ToList());
                    } catch (Exception ex) {
                        _Logger.LogError(ex, "Failed to get characters for logout buffer, trying again in a bit");
                        await Task.Delay(1000 * PERIOD_WAIT_SHORT, stoppingToken);
                        continue;
                    }

                    long censusTime = timer.ElapsedMilliseconds;
                    timer.Restart();

                    int requeued = 0;
                    int left = 0;
                    int notFound = 0;
                    int discarded = 0;

                    ConcurrentQueue<LogoutBufferEntry> queue = new ConcurrentQueue<LogoutBufferEntry>(entries);

                    Task[] tasks = new Task[TASK_COUNT];
                    for (int i = 0; i < TASK_COUNT; ++i) {
                        tasks[i] = Task.Run(async () => {
                            _ = queue.TryDequeue(out LogoutBufferEntry? entry);

                            int count = 0;

                            while (entry != null) {
                                PsCharacter? c = chars.FirstOrDefault(iter => iter.ID == entry.CharacterID);
                                // Sometimes characters are deleted before Census updates them
                                try {
                                    if (c == null) {
                                        Interlocked.Increment(ref notFound);
                                        ++entry.NotFoundCount;

                                        // If Honu fails to find the character 20 times in a row, it's safe to assume it's been deleted from Census
                                        if (entry.NotFoundCount < 20) {
                                            await _LogoutDb.Upsert(entry, stoppingToken);
                                            Interlocked.Increment(ref requeued);
                                        } else {
                                            _Logger.LogWarning($"Missing character {entry.CharacterID} after getting batch. Discarding");
                                            await _LogoutDb.Delete(entry.CharacterID, stoppingToken);
                                            Interlocked.Increment(ref discarded);
                                        }
                                    } else {
                                        TimeSpan diff = entry.LoginTime - c.DateLastLogin;

                                        // Character hasn't been updated, put back in the queue for another check
                                        if (diff > TimeSpan.FromMinutes(5)) {
                                            entry.NotFoundCount = 0; // Character was in Census tho, so reset the missing count
                                            Interlocked.Increment(ref requeued);
                                            await _LogoutDb.Upsert(entry, stoppingToken);
                                        } else {
                                            // Character has been found, don't put back into the queue
                                            //_Logger.LogTrace($"Character {c.Name}/{c.WorldID} has {diff} time span, putting into weapon queue. Took {-(entry.Timestamp - DateTime.UtcNow)} to update, {entry.NotFoundCount}");
                                            Interlocked.Increment(ref left);
                                            await _LogoutDb.Delete(entry.CharacterID, stoppingToken);
                                            _WeaponQueue.Queue(entry.CharacterID);
                                        }
                                    }
                                    ++count;
                                } catch (Exception ex) when (stoppingToken.IsCancellationRequested == false) {
                                    _Logger.LogError(ex, $"failed to update logout entry for {entry.CharacterID} {entry.Timestamp}");
                                    await Task.Delay(100, stoppingToken);
                                } catch (Exception) when (stoppingToken.IsCancellationRequested == true) {

                                }

                                queue.TryDequeue(out entry);
                            }

                            //_Logger.LogTrace($"Task handled {count}/{entries.Count} entries");
                        }, stoppingToken);
                    }

                    await Task.WhenAll(tasks);

                    long processTime = timer.ElapsedMilliseconds;
                    long totalTime = dbTime + censusTime + processTime;
                    int totalDone = requeued + left + discarded;

                    health.RunDuration = totalTime;
                    health.LastRan = DateTime.UtcNow;
                    health.Message = $"Took {totalTime}ms to run for {entries.Count} entries. DB:{dbTime}ms, Census: {censusTime}ms, update: {processTime}ms."
                        + $" {requeued} stayed, {left} removed, {notFound} missing, {discarded} discarded. {totalDone}/{entries.Count} updated";

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
