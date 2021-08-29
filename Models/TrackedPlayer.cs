using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.TagHelpers;

namespace watchtower.Models {

    public class TrackedPlayer {

        /// <summary>
        /// Character ID of a player
        /// </summary>
        public string ID { get; set; } = "";

        public string? OutfitID { get; set; } = null;

        /// <summary>
        /// Faction the character is on
        /// </summary>
        public short FactionID { get; set; }

        /// <summary>
        /// Team the character is on. Different only on NS (FactionID = 4) characters
        /// </summary>
        public short TeamID { get; set; }

        /// <summary>
        /// ID of the server/world the player is on
        /// </summary>
        public short WorldID { get; set; }

        /// <summary>
        /// If the character is online or not
        /// </summary>
        public bool Online { get; set; }

        /// <summary>
        /// Zone ID of the last event the player got
        /// </summary>
        public uint ZoneID { get; set; } = 0;

        /// <summary>
        /// Timestamp (in unix milliseconds) of when the last event a player got. Used for determining AFK players
        /// </summary>
        public long LatestEventTimestamp { get; set; }

    }
}
