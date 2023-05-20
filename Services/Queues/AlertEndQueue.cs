using Microsoft.Extensions.Logging;
using watchtower.Models.Queues;

namespace watchtower.Services.Queues {

    /// <summary>
    ///     Queue for alerts ending
    /// </summary>
    public class AlertEndQueue : BaseQueue<AlertEndQueueEntry> {

        public AlertEndQueue(ILoggerFactory factory) : base(factory) { }

    }

}
