using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Models.Api {

    /// <summary>
    /// Slimmed down version of a character, saving like megabytes and shit
    /// </summary>
    public class SlimCharacter {

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
        /// Tag of the outfit, if the character is in one
        /// </summary>
        public string? OutfitTag { get; set; }

        /// <summary>
        /// Name of the outfit, if the character is in one
        /// </summary>
        public string? OutfitName { get; set; }

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
        public int Prestige { get; set; }

    }
}
