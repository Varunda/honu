using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Models.Db {

    /// <summary>
    ///     Represents how many characters an outfit has online
    /// </summary>
    public class OutfitPopulation {

        /// <summary>
        ///     ID of the outfit
        /// </summary>
        public string? OutfitID { get; set; }

        /// <summary>
        ///     Tag of the outfit
        /// </summary>
        public string? OutfitTag { get; set; }

        /// <summary>
        ///     Name of the outfit
        /// </summary>
        public string OutfitName { get; set; } = "";

        /// <summary>
        ///     Faction ID of the outfit
        /// </summary>
        public short FactionID { get; set; }

        /// <summary>
        ///     How many members the outfit has online
        /// </summary>
        public int Count { get; set; }

    }
}
