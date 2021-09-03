using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Models.Db {

    /// <summary>
    /// Common settings used
    /// </summary>
    public class FacilityControlOptions {

        /// <summary>
        /// Optional, what interval the data will be from
        /// </summary>
        public DateTime? PeriodStart { get; set; }

        /// <summary>
        /// Optional, what interval the data will be to
        /// </summary>
        public DateTime? PeriodEnd { get; set; }

        /// <summary>
        /// Optional, how many players must be present to include in the stats
        /// </summary>
        public int? PlayerThreshold { get; set; }

        /// <summary>
        /// Optional, what zone to get the data on
        /// </summary>
        public uint? ZoneID { get; set; }

        /// <summary>
        /// What world IDs will be included in this data, empty list means all but Jaeger
        /// </summary>
        public List<short> WorldIDs { get; set; } = new();

    }
}
