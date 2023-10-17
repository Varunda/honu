using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Models;
using watchtower.Services.Queues;

namespace watchtower.Services.Hosted {

    public abstract class BaseQueueProcessor<T> : BackgroundService {

        protected readonly ILogger _Logger;
        private readonly BaseQueue<T> _Queue;
        private readonly IServiceHealthMonitor _ServiceHealthMonitor;

        /// <summary>
        ///     Name of the service. Can be disabled using commands
        /// </summary>
        private readonly string _ServiceName;

        // local to just running the queue
        private DateTime? _LastInformedOfDisabled = null;
        private Stopwatch _RunTimer = Stopwatch.StartNew();

        public BaseQueueProcessor(string serviceName, ILoggerFactory factory,
            BaseQueue<T> queue, IServiceHealthMonitor serviceHealthMonitor) {

            _ServiceName = serviceName;

            _Logger = factory.CreateLogger($"watchtower.Services.Hosted.BaseQueueProcessor<{typeof(T).Name}>");

            _Queue = queue;
            _ServiceHealthMonitor = serviceHealthMonitor;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            _Logger.LogInformation($"started, [Service name={_ServiceName}]");

            while (stoppingToken.IsCancellationRequested == false) {
                try {

                    // check if this queue processor is disabled
                    ServiceHealthEntry healthEntry = _ServiceHealthMonitor.GetOrCreate(_ServiceName);
                    if (healthEntry.Enabled == false) {
                        if (_LastInformedOfDisabled == null || (DateTime.UtcNow - _LastInformedOfDisabled) > TimeSpan.FromMinutes(5)) {
                            _Logger.LogInformation($"reminder: service {_ServiceName} is disabled (5m cooldown)");
                            _LastInformedOfDisabled = DateTime.UtcNow;
                        }

                        await Task.Delay(1000, stoppingToken);
                        continue;
                    }

                    T queueEntry = await _Queue.Dequeue(stoppingToken);

                    _RunTimer.Restart();
                    bool toRecord = await _ProcessQueueEntry(queueEntry, stoppingToken);
                    long timeToProcess = _RunTimer.ElapsedMilliseconds;

                    if (toRecord == true) {
                        _Queue.AddProcessTime(timeToProcess);
                    }

                    _ServiceHealthMonitor.Set(_ServiceName, healthEntry);

                } catch (Exception ex) {
                    _Logger.LogError(ex, $"error in queue processor {_ServiceName}");
                }
            }

            _Logger.LogInformation($"stopping");
        }

        /// <summary>
        ///     Process a queue entry, returning true if the duration of processing that entry is to be recorded
        /// </summary>
        /// <param name="entry">Queue entry to be processed</param>
        /// <param name="cancel">Stopping token</param>
        /// <returns>
        ///     A boolean value indicating if the time it took to process this entry will be recorded
        /// </returns>
        protected abstract Task<bool> _ProcessQueueEntry(T entry, CancellationToken cancel);

    }
}
