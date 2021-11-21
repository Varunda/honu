using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models.Census;

namespace watchtower.Models.Api {

    public class ExpandedOutfitMember {

        public OutfitMember Member { get; set; } = new OutfitMember();

        public PsCharacter? Character { get; set; }

        public List<PsCharacterHistoryStat>? Stats { get; set; } = null;

    }
}
