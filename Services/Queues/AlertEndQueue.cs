using Microsoft.Extensions.Logging;
using watchtower.Models.Queues;
using watchtower.Services.Metrics;

namespace watchtower.Services.Queues {

    /// <summary>
    ///     Queue for alerts ending
    /// </summary>
    public class AlertEndQueue : BaseQueue<AlertEndQueueEntry> {

        public AlertEndQueue(ILoggerFactory factory, QueueMetric metrics) : base(factory, metrics) { }

    }

}
