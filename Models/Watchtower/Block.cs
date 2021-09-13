using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Models {

    /// <summary>
    /// A block of data used to display the top players doing certain actions (heals, revives, etc.)
    /// </summary>
    public class Block {

        /// <summary>
        /// Name of the block. Unused
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// The top players who have performed this action
        /// </summary>
        public List<BlockEntry> Entries { get; set; } = new List<BlockEntry>();

        /// <summary>
        /// The total number of actions that have been performed
        /// </summary>
        public int Total { get; set; } = 0;

    }
}
