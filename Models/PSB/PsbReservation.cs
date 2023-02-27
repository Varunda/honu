using System;
using System.Collections.Generic;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Census;

namespace watchtower.Models.PSB {

    /// <summary>
    ///     Information about a PSB reservation
    /// </summary>
    public class PsbReservation {

        /// <summary>
        ///     What outfits are participating
        /// </summary>
        public List<string> Outfits { get; set; } = new();

        /// <summary>
        ///     The contacts for the reservation
        /// </summary>
        public List<PsbOvOContact> Contacts { get; set; } = new();

        /// <summary>
        ///     When the reservation will start
        /// </summary>
        public DateTime Start { get; set; }

        /// <summary>
        ///     When the reservation will end
        /// </summary>
        public DateTime End { get; set; }

        /// <summary>
        ///     How many accounts are requested
        /// </summary>
        public int Accounts { get; set; } = 0;

        /// <summary>
        ///     What bases are requested
        /// </summary>
        public List<PsbBaseBooking> Bases { get; set; } = new();

        /// <summary>
        ///     Any details/notes about the reservation
        /// </summary>
        public string Details { get; set; } = "";

    }

    public class PsbBaseBooking {

        public int FacilityID { get; set; }

        public PsFacility? Facility { get; set; } = null;

        public DateTime Start { get; set; }

        public DateTime End { get; set; }

        public string GetDiscordPretty() {
            return $"{Facility?.Name ?? $"<missing {FacilityID}>"}: {Start.GetDiscordFullTimestamp()} - {End.GetDiscordFullTimestamp()}";
        }

    }

}
