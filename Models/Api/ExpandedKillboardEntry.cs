using watchtower.Models.Census;

namespace watchtower.Models.Api {

    public class ExpandedKillboardEntry {

        public KillboardEntry Entry { get; set; } = new();

        public PsCharacter? Character { get; set; }

    }
}
