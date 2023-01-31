using System;

namespace watchtower.Models.Queues {

    /// <summary>
    ///     Queue entries for when a session is ended
    /// </summary>
    public class SessionEndQueueEntry {

        /// <summary>
        ///     ID of the character whos session ended
        /// </summary>
        public string CharacterID { get; set; } = "";

        /// <summary>
        ///     The session ID that ended. Null if unknown (and not to send to subscriptions)
        /// </summary>
        public long? SessionID { get; set; }

        /// <summary>
        ///     When the session ended
        /// </summary>
        public DateTime Timestamp { get; set; }

    }
}
