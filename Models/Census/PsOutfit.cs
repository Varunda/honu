using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Models.Census {

    public class PsOutfit {

        /// <summary>
        /// ID of the outfit
        /// </summary>
        public string ID { get; set; } = "";

        /// <summary>
        /// Name of the outfit
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// Tag of the outfit
        /// </summary>
        public string? Tag { get; set; }

        /// <summary>
        /// What faction the leader of the outfit is
        /// </summary>
        public short FactionID { get; set; }

        /// <summary>
        /// Character ID of the leader of the outfit
        /// </summary>
        public string LeaderID { get; set; } = "";

        /// <summary>
        /// How many members are in the outfit
        /// </summary>
        public int MemberCount { get; set; }

        /// <summary>
        /// When the outfit was created
        /// </summary>
        public DateTime DateCreated { get; set; }

        /// <summary>
        /// When the outfit was last updated in Honu
        /// </summary>
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    }
}
