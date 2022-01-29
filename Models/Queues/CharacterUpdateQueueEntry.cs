using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models.Census;

namespace watchtower.Models.Queues {

    /// <summary>
    ///     Represents info about performing a character update
    /// </summary>
    public class CharacterUpdateQueueEntry {

        /// <summary>
        ///     ID of the character to perform the update on
        /// </summary>
        public string CharacterID { get; set; } = "";

        /// <summary>
        ///     Character from census. Useful if Honu already got the character (say as part of a logout process),
        ///     and it saves a Census call
        /// </summary>
        public PsCharacter? CensusCharacter { get; set; }

        /// <summary>
        ///     If the character will be updated even if Honu got the data after the last login
        /// </summary>
        public bool Force { get; set; } = false;

        /// <summary>
        ///     Will the refresh be printed (useful for debug)
        /// </summary>
        public bool Print { get; set; } = false;

    }
}
