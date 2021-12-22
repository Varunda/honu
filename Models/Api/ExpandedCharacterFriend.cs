
using watchtower.Models.Census;

namespace watchtower.Models.Api {

    public class ExpandedCharacterFriend {

        public CharacterFriend Entry { get; set; } = new CharacterFriend();

        public PsCharacter? Friend { get; set; }

    }

}