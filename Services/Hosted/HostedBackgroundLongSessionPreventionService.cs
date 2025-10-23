using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Cache;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Code.Tracking;
using watchtower.Models;
using watchtower.Models.Db;
using watchtower.Services.Db;
using watchtower.Services.Queues;
using watchtower.Services.Repositories;

namespace watchtower.Services.Hosted {

    public class HostedBackgroundLongSessionPreventionService : BackgroundService {

        private const string SERVICE_NAME = "long_session_ender";

        private static readonly TimeSpan _RunDelay = TimeSpan.FromSeconds(30);

        private readonly ILogger<HostedBackgroundLongSessionPreventionService> _Logger;

        private readonly IServiceHealthMonitor _ServiceHealthMonitor;
        private readonly SessionRepository _SessionRepository;
        private readonly SessionEndQueue _SessionEndQueue;
        private readonly SessionDbStore _SessionDb;

        public HostedBackgroundLongSessionPreventionService(ILogger<HostedBackgroundLongSessionPreventionService> logger,
            IServiceHealthMonitor serviceHealthMonitor, SessionRepository sessionRepository,
            SessionEndQueue sessionEndQueue, SessionDbStore sessionDb) {

            _Logger = logger;
            _ServiceHealthMonitor = serviceHealthMonitor;
            _SessionRepository = sessionRepository;
            _SessionEndQueue = sessionEndQueue;
            _SessionDb = sessionDb;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            _Logger.LogInformation($"started {SERVICE_NAME}");

            while (!stoppingToken.IsCancellationRequested) {
                try {
                    Stopwatch timer = Stopwatch.StartNew();
                    using Activity? rootTrace = HonuActivitySource.Root.StartActivity("long session ender");

                    ServiceHealthEntry healthEntry = _ServiceHealthMonitor.Get(SERVICE_NAME) ?? new ServiceHealthEntry() {
                        Name = SERVICE_NAME
                    };

                    DateTime now = DateTime.UtcNow;
                    List<Session> cappedSession = await _SessionRepository.GetUnendedOverPeriod(TimeSpan.FromHours(24), stoppingToken);
                    foreach (Session session in cappedSession) {
                        DateTime sessionEnd = new(Math.Min(DateTime.UtcNow.Ticks, (session.Start + TimeSpan.FromHours(24) - TimeSpan.FromSeconds(1)).Ticks));
                        _Logger.LogDebug($"ending session over 24 hours long [sessionID={session.ID}] [start={session.Start:u}] [end={sessionEnd:u}] [diff={now - session.Start}]");

                        await _SessionDb.SetSessionEndByID(session.ID, sessionEnd);

                        TrackedPlayer? player = CharacterStore.Get().GetByCharacterID(session.CharacterID);
                        if (player != null) {
                            _SessionEndQueue.Queue(new Models.Queues.SessionEndQueueEntry() {
                                CharacterID = session.CharacterID,
                                SessionID = session.ID,
                                Timestamp = DateTime.UtcNow
                            });
                        }
                    }

                    healthEntry.LastRan = DateTime.UtcNow;
                    healthEntry.RunDuration = timer.ElapsedMilliseconds;
                    _ServiceHealthMonitor.Set(SERVICE_NAME, healthEntry);
                    rootTrace?.Stop();

                    _Logger.LogInformation($"cleaned up long running sessions (>24h) [count={cappedSession.Count}] [timer={timer.ElapsedMilliseconds}ms]");

                    await Task.Delay(_RunDelay, stoppingToken);
                } catch (Exception) when (stoppingToken.IsCancellationRequested) {
                    _Logger.LogInformation($"service stopped");
                } catch (Exception ex) {
                    _Logger.LogError(ex, "error cleaning up long sessions");
                }
            }
        }

    }
}
