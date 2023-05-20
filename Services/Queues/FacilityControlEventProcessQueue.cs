using Microsoft.Extensions.Logging;
using watchtower.Models.Queues;

namespace watchtower.Services.Queues {

    public class FacilityControlEventProcessQueue : BaseQueue<FacilityControlEventQueueEntry> {

        public FacilityControlEventProcessQueue(ILoggerFactory factory) : base(factory) { }

    }

}
