using DaybreakGames.Census.Exceptions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

        private readonly SessionDbStore _SessionDb;

        public HostedSessionStarterQueue(ILogger<HostedSessionStarterQueue> logger,
            SessionStarterQueue queue, SessionDbStore sessionDb) {

            _Logger = logger;

            _Queue = queue ?? throw new ArgumentNullException(nameof(queue));
            _SessionDb = sessionDb ?? throw new ArgumentNullException(nameof(sessionDb));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            _Logger.LogInformation($"Started {SERVICE_NAME}");

            while (stoppingToken.IsCancellationRequested == false) {
                try {
                    CharacterSessionStartQueueEntry entry = await _Queue.Dequeue(stoppingToken);

                    await _SessionDb.Start(entry.CharacterID, entry.LastEvent);

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
            await _SessionDb.EndAll();

            _Logger.LogDebug($"Took {timer.ElapsedMilliseconds}ms to close all sessions");
        }

    }
}
