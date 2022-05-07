using watchtower.Models.Census;
using watchtower.Models.Events;

namespace watchtower.Models.Api {

    public class ExpandedPlayerControlEvent {

        public PlayerControlEvent Event { get; set; } = new PlayerControlEvent();

        public PsCharacter? Character { get; set; } = null;

    }
}
