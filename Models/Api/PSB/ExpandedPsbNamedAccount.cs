using watchtower.Models.Census;
using watchtower.Models.PSB;

namespace watchtower.Models.Api.PSB {

    public class ExpandedPsbNamedAccount {

        public PsbNamedAccount Account { get; set; } = new PsbNamedAccount();

        public PsCharacter? VsCharacter { get; set; }

        public PsCharacter? NcCharacter { get; set; }

        public PsCharacter? TrCharacter { get; set; }

        public PsCharacter? NsCharacter { get; set; }

    }
}
