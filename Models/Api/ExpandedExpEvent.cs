using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models.Census;
using watchtower.Models.Events;

namespace watchtower.Models.Api {

    public class ExpandedExpEvent {

        public ExpEvent Event { get; set; } = new ExpEvent();

        public PsCharacter? Source { get; set; }

        public PsCharacter? Other { get; set; }

    }
}
