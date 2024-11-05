using Microsoft.Extensions.Logging;
using watchtower.Models.Queues;
using watchtower.Services.Metrics;

namespace watchtower.Services.Queues {

    public class FacilityControlEventProcessQueue : BaseQueue<FacilityControlEventQueueEntry> {

        public FacilityControlEventProcessQueue(ILoggerFactory factory, QueueMetric metrics) : base(factory, metrics) { }

    }

}
