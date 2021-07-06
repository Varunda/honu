using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Models {

    public class TrackedPlayer {

        /// <summary>
        /// Character ID of a player
        /// </summary>
        public string ID { get; set; } = "";

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
        public string ZoneID { get; set; } = "";

        /// <summary>
        /// Timestamp (in unix seconds) of when the last event a player got. Used for determining AFK players
        /// </summary>
        public int LatestEventTimestamp { get; set; }

        /// <summary>
        /// To get an accurate timer of how long a player has been online, we track the intervals the player has been online,
        ///     and to get how many seconds a player has been online, the time period for each each interval is added together.
        ///     
        /// This approach has the benefit that if a player goes offline, then comes back online within the tracking period,
        ///     the time online is accurate (up to how often the interval is checked)
        /// </summary>
        public List<TimestampPair> OnlineIntervals { get; set; } = new List<TimestampPair>();

    }
}
