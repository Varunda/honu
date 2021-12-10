using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models.Census;

namespace watchtower.Models.Api {

    /// <summary>
    ///     Wrapper class around an <see cref="OutfitMember"/>
    /// </summary>
    public class ExpandedOutfitMember {

        /// <summary>
        ///     Wrapped object
        /// </summary>
        public OutfitMember Member { get; set; } = new OutfitMember();

        /// <summary>
        ///     Character of the <see cref="OutfitMember.CharacterID"/> from <see cref="Member"/>
        /// </summary>
        public PsCharacter? Character { get; set; }

        /// <summary>
        ///     Optional list of character stats for the character
        /// </summary>
        public List<PsCharacterHistoryStat>? Stats { get; set; } = null;

    }
}
