using System.Collections.Generic;
using watchtower.Models.Events;

namespace watchtower.Models.Watchtower {

    public class RecentWorldFacilityControlEvents {

        public RecentWorldFacilityControlEvents() {
            Captures = new List<FacilityControlEvent>(11);
            Defenses = new List<FacilityControlEvent>(11);
        }

        public RecentWorldFacilityControlEvents(RecentWorldFacilityControlEvents other) {
            Captures = new List<FacilityControlEvent>(other.Captures);
            Defenses = new List<FacilityControlEvent>(other.Defenses);
        }

        public List<FacilityControlEvent> Captures { get; set; } = new();

        public List<FacilityControlEvent> Defenses { get; set; } = new();

    }

}
