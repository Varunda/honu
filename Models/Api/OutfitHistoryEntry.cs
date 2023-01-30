using System;
using System.Collections.Generic;
using watchtower.Models.Census;

namespace watchtower.Models.Api {

    public class OutfitHistoryEntry {

        public string OutfitID { get; set; } = "";

        public DateTime Start { get; set; }

        public DateTime End { get; set; }

    }

    public class OutfitHistoryBlock {

        public string CharacterID { get; set; } = "";

        public List<OutfitHistoryEntry> Entries { get; set; } = new();

        public List<PsOutfit> Outfits { get; set; } = new();

    }

}
