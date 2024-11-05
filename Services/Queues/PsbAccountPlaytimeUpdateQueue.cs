using Microsoft.Extensions.Logging;
using watchtower.Models.Queues;
using watchtower.Services.Metrics;

namespace watchtower.Services.Queues {

    public class PsbAccountPlaytimeUpdateQueue : BaseQueue<PsbAccountPlaytimeUpdateQueueEntry> {

        public PsbAccountPlaytimeUpdateQueue(ILoggerFactory factory, QueueMetric metrics) : base(factory, metrics) { }

    }

}
