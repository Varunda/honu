using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Models.Db {

    /// <summary>
    /// Options when getting <see cref="ExpDbEntry"/>s
    /// </summary>
    public class ExpEntryOptions {

        /// <summary>
        /// Experience IDs to be included in the returned result
        /// </summary>
        public List<int> ExperienceIDs { get; set; } = new List<int>();

        /// <summary>
        /// Faction ID to limit the results to
        /// </summary>
        public short FactionID { get; set; }

        /// <summary>
        /// How many minutes back will this data be built from
        /// </summary>
        public int Interval { get; set; }

        /// <summary>
        /// What world to get the data from
        /// </summary>
        public short WorldID { get; set; }

    }
}
