using watchtower.Models.Census;

namespace watchtower.Models.Queues {

    /// <summary>
    ///     Queue entry for creating the stats of an alert once an alert has ended
    /// </summary>
    public class AlertEndQueueEntry {

        /// <summary>
        ///     Alert that ended
        /// </summary>
        public PsAlert Alert { get; set; } = new();

    }
}
