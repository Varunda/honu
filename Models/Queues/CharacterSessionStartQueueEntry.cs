using System;

namespace watchtower.Models.Queues {

    /// <summary>
    ///     Contains the parameters for when a character's session is started
    /// </summary>
    public class CharacterSessionStartQueueEntry {

        /// <summary>
        ///     Character ID of the character to have their session started
        /// </summary>
        public string CharacterID { get; set; } = "";

        /// <summary>
        ///     When this character was first seen, and when the session will be started
        /// </summary>
        public DateTime LastEvent { get; set; }

    }
}
