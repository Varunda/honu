using System;
using watchtower.Models.Census;

namespace watchtower.Models.OutfitWarsNexus {

    /// <summary>
    ///     Information about an outfit wars match
    /// </summary>
    public class OutfitWarsMatch {

        /// <summary>
        ///     When this match started
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        ///     ID of the world this match is taking place on
        /// </summary>
        public short WorldID { get; set; }

        /// <summary>
        ///     Zone ID of where this match is taking place
        /// </summary>
        public uint ZoneID { get; set; }

        /// <summary>
        ///     Null if the outfit is currently unknown
        /// </summary>
        public OutfitWarsTeam? TeamAlpha { get; set; } = null;

        /// <summary>
        ///     Null if the outfit is currently unknown
        /// </summary>
        public OutfitWarsTeam? TeamOmega { get; set; } = null;

    }

}
