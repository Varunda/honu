using System;
using System.Collections.Generic;
using System.Linq;
using watchtower.Code.ExtensionMethods;
using watchtower.Constants;
using watchtower.Models.Census;

namespace watchtower.Models.PSB {

    /// <summary>
    ///     Information about a PSB reservation
    /// </summary>
    public class PsbReservation {

        public string Name {
            get {
                return $"{Start:yyyy-MM-dd}: {string.Join(" / ", Outfits)}";
            }
        }

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

        public PsbBaseBooking() {

        }

        /// <summary>
        ///     copy ctor
        /// </summary>
        /// <param name="other"></param>
        public PsbBaseBooking(PsbBaseBooking other) {
            Facilities = new List<PsFacility>(other.Facilities);
            ZoneID = other.ZoneID;
            Start = other.Start;
            End = other.End;
        }

        /// <summary>
        ///     List of facilities that are in this base booking
        /// </summary>
        public List<PsFacility> Facilities { get; set; } = new();

        /// <summary>
        ///     If this booking is for a whole zone, this value will be set
        /// </summary>
        public uint? ZoneID { get; set; } = null;

        /// <summary>
        ///     When this reservation starts. Must be on the same day as <see cref="End"/>
        /// </summary>
        public DateTime Start { get; set; }

        /// <summary>
        ///     When this reservation ends. Must be on the same day as <see cref="Start"/>
        /// </summary>
        public DateTime End { get; set; }

        public string GetDiscordPretty() {
            string entity = "";

            if (ZoneID == null) {
                entity = string.Join(", ", Facilities.Select(iter => iter.Name));
            } else {
                entity = Zone.GetName(ZoneID.Value) + " (Continent)";
            }

            return $"{entity}: {Start.GetDiscordFullTimestamp()} - {End.GetDiscordFullTimestamp()}";
        }

    }

}
