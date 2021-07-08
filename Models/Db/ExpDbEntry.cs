using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Models.Db {

    /// <summary>
    /// Represents how many times a player or outfit has performed an action
    /// </summary>
    public class ExpDbEntry {

        /// <summary>
        ///     ID of either the player or the outfit that performed the action.
        ///     If it's an character ID or outfit ID depends on the context of where the entry is created
        /// </summary>
        public string ID { get; set; } = "";

        /// <summary>
        /// How many actions the character/outfit has performed
        /// </summary>
        public int Count { get; set; }

    }
}
