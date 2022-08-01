using DaybreakGames.Census.Exceptions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Code.Tracking;
using watchtower.Constants;
using watchtower.Models;
using watchtower.Models.Census;
using watchtower.Models.Queues;
using watchtower.Services.Db;
using watchtower.Services.Queues;
using watchtower.Services.Repositories;

namespace watchtower.Services.Hosted {

    /// <summary>
    ///     Hosted queue that starts sessions for characters
    /// </summary>
    public class HostedSessionStarterQueue : BackgroundService {

        private const string SERVICE_NAME = "background_session_queue";

        private readonly ILogger<HostedSessionStarterQueue> _Logger;
        private readonly SessionStarterQueue _Queue;

        private readonly SessionRepository _SessionRepository;
        private readonly CharacterRepository _CharacterRepository;
        private readonly CharacterCacheQueue _CharacterCacheQueue;

        public HostedSessionStarterQueue(ILogger<HostedSessionStarterQueue> logger,
            SessionStarterQueue queue, CharacterRepository characterRepository,
            CharacterCacheQueue characterCacheQueue, SessionRepository sessionRepository) {

            _Logger = logger;

            _Queue = queue ?? throw new ArgumentNullException(nameof(queue));
            _CharacterRepository = characterRepository;
            _CharacterCacheQueue = characterCacheQueue;
            _SessionRepository = sessionRepository;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            _Logger.LogInformation($"Started {SERVICE_NAME}");

            Stopwatch timer = Stopwatch.StartNew();

            while (stoppingToken.IsCancellationRequested == false) {
                try {
                    CharacterSessionStartQueueEntry entry = await _Queue.Dequeue(stoppingToken);
                    timer.Restart();

                    using (Activity? start = HonuActivitySource.Root.StartActivity("session start")) {
                        using Activity? getCharacter = HonuActivitySource.Root.StartActivity("get char");
                        PsCharacter? c = await _CharacterRepository.GetByID(entry.CharacterID);
                        getCharacter?.Stop();
                        if (c == null) {
                            ++entry.FailCount;
                            _Logger.LogInformation($"Character {entry.CharacterID} does not exist locally, queue character cache and requeueing session start, failed {entry.FailCount} times");

                            if (entry.FailCount <= 50) {
                                _Queue.Queue(entry);
                                _CharacterCacheQueue.Queue(entry.CharacterID);
                                continue;
                            }
                        }

                        if (entry.FailCount > 0) {
                            _Logger.LogDebug($"Took {entry.FailCount} tries to find the character locally");
                        }

                        using (Activity? repoCall = HonuActivitySource.Root.StartActivity("repo start")) {
                            await _SessionRepository.Start(entry.CharacterID, entry.LastEvent, c?.OutfitID ?? "0", c?.FactionID ?? 0);
                        }
                    }

                    _Queue.AddProcessTime(timer.ElapsedMilliseconds);
                } catch (Exception ex) when (stoppingToken.IsCancellationRequested == false) {
                    _Logger.LogError(ex, "Error starting session in the background");
                } catch (Exception) when (stoppingToken.IsCancellationRequested == true) {
                    _Logger.LogInformation($"Stopping {SERVICE_NAME} with {_Queue.Count()} left");
                }
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken) {
            _Logger.LogInformation($"Ending all current sessions");
            Stopwatch timer = Stopwatch.StartNew();
            await _SessionRepository.EndAll(DateTime.UtcNow);

            _Logger.LogDebug($"Took {timer.ElapsedMilliseconds}ms to close all sessions");
        }

    }
}
