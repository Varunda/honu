using watchtower.Models.Census;
using watchtower.Models.PSB;

namespace watchtower.Models.Api.PSB {

    public class ExpandedPsbAccount {

        public PsbAccount Account { get; set; } = new PsbAccount();

        public PsCharacter? VsCharacter { get; set; }

        public PsCharacter? NcCharacter { get; set; }

        public PsCharacter? TrCharacter { get; set; }

        public PsCharacter? NsCharacter { get; set; }

    }
}
