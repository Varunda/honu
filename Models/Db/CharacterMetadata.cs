using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Models.Db {

    /// <summary>
    ///     Represents meta information about a character not saved in Census
    /// </summary>
    public class CharacterMetadata {

        /// <summary>
        ///     ID of the character this metadata is for
        /// </summary>
        public string ID { get; set; } = "";

        /// <summary>
        ///     When the character was last put in the background queue and has all it's stats updated
        /// </summary>
        public DateTime LastUpdated { get; set; }

        /// <summary>
        ///     How many times the character has not been found in Census, potentially meaning the character is deleted
        /// </summary>
        public int NotFoundCount { get; set; }

    }
}
