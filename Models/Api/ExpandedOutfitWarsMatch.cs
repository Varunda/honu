using watchtower.Models.Census;

namespace watchtower.Models.Api {

    public class ExpandedOutfitWarsMatch {

        public OutfitWarsMatch Match { get; set; } = new();

        public PsOutfit? OutfitA { get; set; }

        public PsOutfit? OutfitB { get; set; }

    }
}
