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

        /// <summary>
        ///     don't start a session until after this time. used for timeout in session starting
        /// </summary>
        public DateTime Backoff { get; set; }

        /// <summary>
        ///     if Honu fails to get a character from Census, we do want to create the DB record as soon as the session starts,
        ///     rather than once we get the character. this field lets the session start queue update a session instead of creating a new one
        /// </summary>
        public long? SessionID { get; set; }

    }
}
