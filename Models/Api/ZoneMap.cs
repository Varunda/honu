using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models.Census;

namespace watchtower.Models.Api {

    /// <summary>
    ///     Container class that has the hexes, facilities and lattice links of a zone
    /// </summary>
    public class ZoneMap {

        /// <summary>
        ///     Hexes in a zone
        /// </summary>
        public List<PsMapHex> Hexes { get; set; } = new List<PsMapHex>();

        /// <summary>
        ///     Facilities in a zone
        /// </summary>
        public List<PsFacility> Facilities { get; set; } = new List<PsFacility>();

        /// <summary>
        ///     Lattice links in a zone
        /// </summary>
        public List<PsFacilityLink> Links { get; set; } = new List<PsFacilityLink>();

    }
}
