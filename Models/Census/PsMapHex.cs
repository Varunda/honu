using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Models.Census {

    /// <summary>
    ///     Represents a hex of a zone
    /// </summary>
    public class PsMapHex {

        /// <summary>
        ///     What region this hex is in. Not unique
        /// </summary>
        public int RegionID { get; set; }

        /// <summary>
        ///     What zone this hex is in
        /// </summary>
        public uint ZoneID { get; set; }

        /// <summary>
        ///     Hex X coordinate
        /// </summary>
        public int X { get; set; }

        /// <summary>
        ///     Hex Y coordinate
        /// </summary>
        public int Y { get; set; }

        /// <summary>
        ///     Type of the hex
        /// </summary>
        public int HexType { get; set; }

    }
}
