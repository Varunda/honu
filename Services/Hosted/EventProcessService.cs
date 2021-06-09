using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Realtime;

namespace watchtower.Services {

    public class EventProcessService : BackgroundService {

        private readonly ILogger<EventProcessService> _Logger;
        private readonly IBackgroundTaskQueue _Queue;
        private readonly IEventHandler _Handler;

        public EventProcessService( ILogger<EventProcessService> logger,
            IBackgroundTaskQueue queue, IEventHandler handler) {

            _Logger = logger;

            _Queue = queue;
            _Handler = handler;
        }

        protected async override Task ExecuteAsync(CancellationToken cancel) {
            while (cancel.IsCancellationRequested == false) {
                JToken token = await _Queue.DequeueAsync(cancel);
                try {
                    _Handler.Process(token);
                } catch (Exception ex) {
                    _Logger.LogError(ex, "Failed to process {token}", token);
                }
            }
        }

    }
}
