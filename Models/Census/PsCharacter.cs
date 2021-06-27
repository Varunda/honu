using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Models.Census {

    public class PsCharacter {

        /// <summary>
        /// ID of the character
        /// </summary>
        public string ID { get; set; } = "";

        /// <summary>
        /// Name of the character
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// ID of the world the character is one
        /// </summary>
        public short WorldID { get; set; }

        /// <summary>
        /// ID of the outfit, if the character is in one
        /// </summary>
        public string? OutfitID { get; set; }

        /// <summary>
        /// Faction ID of the character
        /// </summary>
        public short FactionID { get; set; }

        /// <summary>
        /// Battle rank of the character
        /// </summary>
        public short BattleRank { get; set; }

        /// <summary>
        /// Has the character ASPed?
        /// </summary>
        public bool Prestige { get; set; }

        /// <summary>
        /// <c>DateTime</c> of when the last update on this character occured
        /// </summary>
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    }
}
