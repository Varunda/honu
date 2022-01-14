using System;

namespace watchtower.Models.PSB {

    /// <summary>
    ///     Notes about a PSB account
    /// </summary>
    public class PsbAccountNote {

        /// <summary>
        ///     ID of the message
        /// </summary>
        public long ID { get; set; }

        /// <summary>
        ///     ID of the PSB account
        /// </summary>
        public long AccountID { get; set; }

        /// <summary>
        ///     ID of the Honu account that added this note
        /// </summary>
        public long HonuID { get; set; }

        /// <summary>
        ///     Timestamp of when the message was created
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        ///     Message of the message :)
        /// </summary>
        public string Message { get; set; } = "";

        /// <summary>
        ///     If this message was deleted, when was that?
        /// </summary>
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        ///     ID of the honu account that deleted this note
        /// </summary>
        public long? DeletedBy { get; set; }

        /// <summary>
        ///     When this message was last edited
        /// </summary>
        public DateTime? EditedAt { get; set; }

    }
}
