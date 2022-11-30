using watchtower.Models.PSB;

namespace watchtower.Models.Queues {

    public class PsbAccountPlaytimeUpdateQueueEntry {

        /// <summary>
        ///     ID of the <see cref="PsbNamedAccount"/> to update the playtime of
        /// </summary>
        public long AccountID { get; set; }

    }
}
