using Microsoft.Extensions.Logging;
using watchtower.Models.Queues;

namespace watchtower.Services.Queues {

    public class PsbAccountPlaytimeUpdateQueue : BaseQueue<PsbAccountPlaytimeUpdateQueueEntry> {

        public PsbAccountPlaytimeUpdateQueue(ILoggerFactory factory) : base(factory) { }

    }

}
