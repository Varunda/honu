using Microsoft.Extensions.Logging;
using watchtower.Models.Queues;

namespace watchtower.Services.Queues {

    /// <summary>
    ///     Queue for when a session is ended. Used for subscriptions
    /// </summary>
    public class SessionEndQueue : BaseQueue<SessionEndQueueEntry> {

        public SessionEndQueue(ILoggerFactory factory) : base(factory) { }

    }
}
