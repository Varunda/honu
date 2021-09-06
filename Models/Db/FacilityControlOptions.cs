using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Code.Constants;

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
        /// Optional, how many players must be present to include in the stats, defaults to 12
        /// </summary>
        public int PlayerThreshold { get; set; } = 12;

        /// <summary>
        /// Optional, what zone to get the data on
        /// </summary>
        public uint? ZoneID { get; set; }

        /// <summary>
        /// Will control events from single or double lane zones be included? Or just when the map is fully open?
        /// </summary>
        public UnstableState UnstableState { get; set; } = UnstableState.UNLOCKED;

        /// <summary>
        /// What world IDs will be included in this data, empty list means all but Jaeger
        /// </summary>
        public List<short> WorldIDs { get; set; } = new();

    }
}
