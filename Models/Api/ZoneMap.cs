using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models.Census;

namespace watchtower.Models.Api {

    public class ZoneMap {

        public List<PsMapHex> Hexes { get; set; } = new List<PsMapHex>();

        public List<PsFacility> Facilities { get; set; } = new List<PsFacility>();

        public List<PsFacilityLink> Links { get; set; } = new List<PsFacilityLink>();

    }
}
