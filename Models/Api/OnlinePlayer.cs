using watchtower.Models.Census;

namespace watchtower.Models.Api {

    public class OnlinePlayer {

        public string CharacterID { get; set; } = "";

        public PsCharacter? Character { get; set; }

        public TrackedPlayer? Player { get; set; }

    }
}
