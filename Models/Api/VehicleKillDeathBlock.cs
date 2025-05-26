using System.Collections.Generic;
using watchtower.Models.Census;
using watchtower.Models.Events;

namespace watchtower.Models.Api {

    public class VehicleKillDeathBlock {

        public List<VehicleDestroyEvent> Kills { get; set; } = [];

        public List<VehicleDestroyEvent> Deaths { get; set; } = [];

        public List<PsCharacter> Characters { get; set; } = [];

        public List<PsItem> Weapons { get; set; } = [];

        public List<ItemCategory> ItemCategories { get; set; } = [];

        public List<PsVehicle> Vehicles { get; set; } = [];

    }
}
