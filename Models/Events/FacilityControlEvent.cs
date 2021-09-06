using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Code.Constants;

namespace watchtower.Models.Events {

    public class FacilityControlEvent {

        /// <summary>
        /// ID of the Facility
        /// </summary>
        public int FacilityID { get; set; }

        /// <summary>
        /// UTC 
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// From Census
        /// </summary>
        public int DurationHeld { get; set; }

        /// <summary>
        /// How many players got the capture/defend events, filled in after creation
        /// </summary>
        public int Players { get; set; }

        /// <summary>
        /// Is the continent fully unlocked?
        /// </summary>
        public UnstableState? UnstableState { get; set; }

        /// <summary>
        /// Faction that completed the control. If it's the same as <see cref="OldFactionID"/>, it's a defense
        /// </summary>
        public short NewFactionID { get; set; }

        /// <summary>
        /// Old faction that held the base when the control started
        /// </summary>
        public short OldFactionID { get; set; }

        /// <summary>
        /// Outfit that took the base
        /// </summary>
        public string? OutfitID { get; set; }

        /// <summary>
        /// What world this event took place at
        /// </summary>
        public short WorldID { get; set; }

        /// <summary>
        /// What zone the facility is in, useful for instanced zones like Deso
        /// </summary>
        public uint ZoneID { get; set; }

    }
}
