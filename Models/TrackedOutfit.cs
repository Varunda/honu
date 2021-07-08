using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Models {

    /// <summary>
    /// Block of data that holds the top outfit killers
    /// </summary>
    public class OutfitKillBlock {

        /// <summary>
        /// Entries
        /// </summary>
        public List<TrackedOutfit> Entries { get; set; } = new List<TrackedOutfit>();

    }

    public class TrackedOutfit {

        /// <summary>
        /// ID of the outfit
        /// </summary>
        public string ID { get; set; } = "";

        /// <summary>
        /// Tag/Alias of the outfit
        /// </summary>
        public string? Tag { get; set; }

        /// <summary>
        /// Name of the outfit
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// What faction the leader of the outfit is on
        /// </summary>
        public string FactionID { get; set; } = "";

        /// <summary>
        /// How many kills the members of this outfit have gotten
        /// </summary>
        public int Kills { get; set; }

        /// <summary>
        /// How many deaths the members of this outfit have had
        /// </summary>
        public int Deaths { get; set; }

        /// <summary>
        /// How many members are in the outfit
        /// </summary>
        public int Members { get; set; }

        /// <summary>
        /// How many members are currently online in the outfit
        /// </summary>
        public int MembersOnline { get; set; }

    }
}
