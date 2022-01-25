using System.Collections.Generic;

namespace watchtower.Models.CharacterViewer.CharacterStats {

    public class ExtraStatSet {

        public string Name { get; set; } = "";

        public string Description { get; set; } = "";

        public List<CharacterStatBase> Stats { get; set; } = new List<CharacterStatBase>();

    }
}
