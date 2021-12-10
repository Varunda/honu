using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Models.Census {

    /// <summary>
    ///     Represents information about a facility/base
    /// </summary>
    public class PsFacility {

        /// <summary>
        ///     ID of the facility
        /// </summary>
        public int FacilityID { get; set; }

        /// <summary>
        ///     Zone the facility is in
        /// </summary>
        public uint ZoneID { get; set; }

        /// <summary>
        ///     Region the facility is in
        /// </summary>
        public int RegionID { get; set; }

        /// <summary>
        ///     Name of the facility
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        ///     ID of the facility type (small/large/tech plant/etc.)
        /// </summary>
        public int TypeID { get; set; }

        /// <summary>
        ///     Name of the facility type
        /// </summary>
        public string TypeName { get; set; } = "";

        /// <summary>
        ///     Where in the zone the facility is located
        /// </summary>
        public decimal? LocationX { get; set; }

        /// <summary>
        ///     Where in the zone the facility is located
        /// </summary>
        public decimal? LocationY { get; set; }

        /// <summary>
        ///     Where in the zone the facility is located
        /// </summary>
        public decimal? LocationZ { get; set; }
        
    }
}
