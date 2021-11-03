using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models.Census;
using watchtower.Models.Db;

namespace watchtower.Models.Api {

    public class ExpandedSession {

        public Session Session { get; set; } = new Session();

        public PsCharacter? Character { get; set; }

        public PsOutfit? Outfit { get; set; }

    }
}
