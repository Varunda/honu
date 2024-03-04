using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using watchtower.Code.Constants;
using watchtower.Models.Events;

namespace watchtower.Models {

    /// <summary>
    ///     Represents information about a player that is tracked in Honu
    /// </summary>
    public class TrackedPlayer {

        /// <summary>
        ///     Character ID of a player
        /// </summary>
        public string ID { get; set; } = "";

        /// <summary>
        ///     ID of the outfit the character is in
        /// </summary>
        public string? OutfitID { get; set; } = null;

        /// <summary>
        ///     Faction the character is on
        /// </summary>
        public short FactionID { get; set; }

        /// <summary>
        ///     Team the character is on. Different only on NS (FactionID = 4) characters
        /// </summary>
        public short TeamID { get; set; }

        /// <summary>
        ///     ID of the server/world the player is on
        /// </summary>
        public short WorldID { get; set; }

        /// <summary>
        ///     If the character is online or not
        /// </summary>
        public bool Online { get; set; }

        /// <summary>
        ///     Zone ID of the last event the player got
        /// </summary>
        public uint ZoneID { get; set; } = 0;

        /// <summary>
        ///     Timestamp (in unix milliseconds) of when the last event a player got. Used for determining AFK players
        /// </summary>
        public long LatestEventTimestamp { get; set; }

        /// <summary>
        ///     Last death of the character. Used to update when a character is revived
        /// </summary>
        public KillEvent? LatestDeath { get; set; }

        /// <summary>
        ///     ID of the session a character is currently in
        /// </summary>
        public long? SessionID { get; set; }

        /// <summary>
        ///     When the character last logged in, or <c>null</c> if unknown (such as restarting Honu)
        /// </summary>
        public DateTime? LastLogin { get; set; }

        /// <summary>
        ///     the ID of the vehicle this player might be in. there are 2 special values: 0 means no value, and -1 means in a vehicle, but unknown
        /// </summary>
        public int PossibleVehicleID { get; set; }

        /// <summary>
        ///     what profile ID (<see cref="Profile"/>) this character was last
        /// </summary>
        public short ProfileID { get; set; }

    }
}
