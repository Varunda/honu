using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Constants;

namespace watchtower.Models {

    public class NsoTrackingEntry {

        public string CharacterID { get; set; } = "";

        public bool CanBeTR { get; set; }

        public bool CanBeNC { get; set; }

        public bool CanBeVS { get; set; }

        public short TeamID { get; set; } = Faction.NS;

    }
}
