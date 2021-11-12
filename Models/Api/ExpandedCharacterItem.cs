using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models.Census;

namespace watchtower.Models.Api {

    public class ExpandedCharacterItem {

        public CharacterItem Entry { get; set; } = new CharacterItem();

        public PsItem? Item { get; set; }

    }
}
