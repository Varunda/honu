using watchtower.Models.Census;
using watchtower.Models.Events;

namespace watchtower.Models.Api {

    public class ExpandedVehicleDestroyEvent {

        public VehicleDestroyEvent Event { get; set; } = new VehicleDestroyEvent();

        public PsCharacter? Attacker { get; set; } = null;

        public PsCharacter? Killed { get; set; } = null;

        public PsItem? Item { get; set; }

        public PsVehicle? AttackerVehicle { get; set; }

        public PsVehicle? KilledVehicle { get; set; }

    }
}
