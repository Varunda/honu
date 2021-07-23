using DaybreakGames.Census.Exceptions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Constants;
using watchtower.Models;
using watchtower.Models.Census;
using watchtower.Services.Db;
using watchtower.Services.Repositories;

namespace watchtower.Services.Hosted {

    public class HostedSessionStarterQueue : BackgroundService {

        private const string SERVICE_NAME = "background_session_queue";

        private readonly ILogger<HostedSessionStarterQueue> _Logger;
        private readonly IBackgroundSessionStarterQueue _Queue;

        private readonly ISessionDbStore _SessionDb;

        public HostedSessionStarterQueue(ILogger<HostedSessionStarterQueue> logger,
            IBackgroundSessionStarterQueue queue, ISessionDbStore sessionDb) {

            _Logger = logger;

            _Queue = queue ?? throw new ArgumentNullException(nameof(queue));
            _SessionDb = sessionDb ?? throw new ArgumentNullException(nameof(sessionDb));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            while (stoppingToken.IsCancellationRequested == false) {
                try {
                    TrackedPlayer player = await _Queue.DequeueAsync(stoppingToken);

                    await _SessionDb.Start(player);

                } catch (Exception ex) when (stoppingToken.IsCancellationRequested == false) {
                    _Logger.LogError(ex, "Error starting session in the background");
                } catch (Exception) when (stoppingToken.IsCancellationRequested == true) {
                    _Logger.LogInformation($"Stopping {SERVICE_NAME}");
                }
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken) {
            _Logger.LogInformation($"Ending all current sessions");
            await _SessionDb.EndAll();
        }

    }
}
