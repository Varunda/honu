using watchtower.Models.Census;
using watchtower.Models.Events;

namespace watchtower.Models.Api {

    public class ExpandedFacilityControlEvent {

        public FacilityControlEvent Event { get; set; } = new FacilityControlEvent();

        public PsOutfit? Outfit { get; set; } = null;

        public PsFacility? Facility { get; set; } = null;

    }
}
