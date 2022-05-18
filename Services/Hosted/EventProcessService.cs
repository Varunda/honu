using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Code;
using watchtower.Realtime;
using watchtower.Services.Queues;

namespace watchtower.Services {

    public class EventProcessService : BackgroundService {

        private readonly ILogger<EventProcessService> _Logger;
        private readonly CensusRealtimeEventQueue _Queue;
        private readonly IEventHandler _Handler;

        public EventProcessService(ILogger<EventProcessService> logger,
            CensusRealtimeEventQueue queue, IEventHandler handler) {

            _Logger = logger;

            _Queue = queue;
            _Handler = handler;
        }

        protected async override Task ExecuteAsync(CancellationToken cancel) {
            Stopwatch timer = Stopwatch.StartNew();
            while (cancel.IsCancellationRequested == false) {
                JToken token = await _Queue.Dequeue(cancel);
                try {
                    timer.Restart();
                    await _Handler.Process(token);
                    _Queue.AddProcessTime(timer.ElapsedMilliseconds);
                    if (Logging.EventProcess == true && timer.ElapsedMilliseconds > 100) {
                        _Logger.LogWarning($"Took {timer.ElapsedMilliseconds}ms to process {token}");
                    }
                } catch (Exception ex) when (cancel.IsCancellationRequested == false) {
                    _Logger.LogError(ex, "Failed to process {token}", token);
                } catch (Exception) when (cancel.IsCancellationRequested == true) {
                    _Logger.LogInformation($"Stopping event_process with {_Queue.Count()} left");
                }
            }
        }

    }
}
