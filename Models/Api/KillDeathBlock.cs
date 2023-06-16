using System.Collections.Generic;
using watchtower.Models.Census;
using watchtower.Models.Events;

namespace watchtower.Models.Api {

    public class KillDeathBlock {

        public List<KillEvent> Kills { get; set; } = new();

        public List<KillEvent> Deaths { get; set; } = new();

        public List<PsCharacter> Characters { get; set; } = new();

        public List<PsItem> Weapons { get; set; } = new();

        public List<FireGroupToFireMode> FireModes { get; set; } = new();

        public List<ItemCategory> ItemCategories { get; set; } = new();

    }
}
