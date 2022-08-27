using System;
using watchtower.Code.Constants;

namespace watchtower.Models.Queues {

    /// <summary>
    ///     Contains the parameters for when a character's session is started
    /// </summary>
    public class CharacterSessionStartQueueEntry {

        /// <summary>
        ///     Character ID of the character to have their session started
        /// </summary>
        public string CharacterID { get; set; } = "";

        public CensusEnvironment Environment { get; set; }

        /// <summary>
        ///     When this character was first seen, and when the session will be started
        /// </summary>
        public DateTime LastEvent { get; set; }

        /// <summary>
        ///     How many times Honu failed to start the session
        /// </summary>
        public int FailCount { get; set; } = 0;

        public DateTime Backoff { get; set; }

    }
}
