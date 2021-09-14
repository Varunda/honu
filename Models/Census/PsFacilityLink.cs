using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Models.Census {

    public class PsFacilityLink {

        public uint ZoneID { get; set; }

        public int FacilityA { get; set; }

        public int FacilityB { get; set; }

        public string? Description { get; set; }

    }
}
