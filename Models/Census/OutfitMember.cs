using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Models.Census {

    /// <summary>
    ///     Represents data from the /outfit_member collection, joined to the characters_world collection,
    ///     as sometimes during server transfers outfits can get split
    /// </summary>
    public class OutfitMember {

        /// <summary>
        ///     ID of the outfit this member is in
        /// </summary>
        public string OutfitID { get; set; } = "";

        /// <summary>
        ///     ID of the character
        /// </summary>
        public string CharacterID { get; set; } = "";

        /// <summary>
        ///     When this member joined the outfit
        /// </summary>
        public DateTime MemberSince { get; set; }

        /// <summary>
        ///     Rank name of the member
        /// </summary>
        public string Rank { get; set; } = "";

        /// <summary>
        ///     What order the rank this member is, is
        /// </summary>
        public int RankOrder { get; set; }

        /// <summary>
        ///     ID of the world this member is in. Useful during server merges/splits
        ///     and some members go to one server, while others go to another
        /// </summary>
        public short? WorldID { get; set; }

    }
}
