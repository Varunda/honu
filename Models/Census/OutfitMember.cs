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

        public string OutfitID { get; set; } = "";

        public string CharacterID { get; set; } = "";

        public DateTime MemberSince { get; set; }

        public string Rank { get; set; } = "";

        public int RankOrder { get; set; }

        public short WorldID { get; set; }

    }
}
