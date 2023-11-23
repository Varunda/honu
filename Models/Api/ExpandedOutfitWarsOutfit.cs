using watchtower.Models.Census;

namespace watchtower.Models.Api {

    public class ExpandedOutfitWarsOutfit {

        public OutfitWarsOutfit Entry { get; set; } = new();

        public PsOutfit? Outfit { get; set; } = null;

    }
}
