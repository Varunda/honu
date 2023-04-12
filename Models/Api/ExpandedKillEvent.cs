using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models.Census;
using watchtower.Models.Events;

namespace watchtower.Models.Api {

    public class ExpandedKillEvent {

        public KillEvent Event { get; set; } = new KillEvent();

        public PsCharacter? Attacker { get; set; }

        public PsCharacter? Killed { get; set; }

        public PsItem? Item { get; set; }

        public FireGroupToFireMode? FireGroupToFireMode { get; set; }

    }
}
