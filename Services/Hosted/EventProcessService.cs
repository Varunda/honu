using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Realtime;

namespace watchtower.Services {

    public class EventProcessService : BackgroundService {

        private readonly ILogger<EventProcessService> _Logger;
        private readonly IBackgroundTaskQueue _Queue;
        private readonly IEventHandler _Handler;

        public EventProcessService(ILogger<EventProcessService> logger,
            IBackgroundTaskQueue queue, IEventHandler handler) {

            _Logger = logger;

            _Queue = queue;
            _Handler = handler;
        }

        protected async override Task ExecuteAsync(CancellationToken cancel) {
            Stopwatch timer = Stopwatch.StartNew();
            while (cancel.IsCancellationRequested == false) {
                JToken token = await _Queue.DequeueAsync(cancel);
                try {
                    timer.Restart();
                    await _Handler.Process(token);
                    if (timer.ElapsedMilliseconds > 100) {
                        //_Logger.LogWarning($"Took {timer.ElapsedMilliseconds}ms to process {token}");
                    }
                } catch (Exception ex) {
                    _Logger.LogError(ex, "Failed to process {token}", token);
                }
            }
        }

    }
}
