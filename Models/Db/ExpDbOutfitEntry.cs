using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Models.Db {

    /// <summary>
    /// Contains fields about how many times characters within an outfit have performed an action (such as heals, revives, etc.)
    /// </summary>
    public class ExpDbOutfitEntry {

        /// <summary>
        /// ID of the outfit 
        /// </summary>
        public string OutfitID { get; set; } = "";

        /// <summary>
        /// How many of the action the outfit has performed
        /// </summary>
        public int Count { get; set; }

    }
}
