using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Models {

    /// <summary>
    /// Represents state data of a zone
    /// </summary>
    public class ZoneState {

        /// <summary>
        /// ID of the zone
        /// </summary>
        public uint ZoneID { get; set; }

        /// <summary>
        /// What world this zone datat is for
        /// </summary>
        public short WorldID { get; set; }

        /// <summary>
        /// Is this zone currently open
        /// </summary>
        public bool IsOpened { get; set; } = false;

        /// <summary>
        /// When did an alert on this zone start?
        /// </summary>
        public DateTime? AlertStart { get; set; }

        /// <summary>
        /// When will the alert on this zone end, useful for like deso
        /// </summary>
        public DateTime? AlertEnd { get; set; }

    }
}
