using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Models.Census {

    /// <summary>
    ///     Represents a link between two facilities
    /// </summary>
    public class PsFacilityLink {

        /// <summary>
        ///     ID of the zone the lattice link is in
        /// </summary>
        public uint ZoneID { get; set; }

        /// <summary>
        ///     ID of the first facility
        /// </summary>
        public int FacilityA { get; set; }

        /// <summary>
        ///     ID of the 2nd facility
        /// </summary>
        public int FacilityB { get; set; }

        /// <summary>
        ///     Optional description, sometimes in Census, sometimes not
        /// </summary>
        public string? Description { get; set; }

    }
}
