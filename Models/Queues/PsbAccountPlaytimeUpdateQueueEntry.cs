using watchtower.Models.PSB;

namespace watchtower.Models.Queues {

    public class PsbAccountPlaytimeUpdateQueueEntry {

        /// <summary>
        ///     ID of the <see cref="PsbAccount"/> to update the playtime of
        /// </summary>
        public long AccountID { get; set; }

    }
}
