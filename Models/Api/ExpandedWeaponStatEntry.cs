using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models.Census;
using watchtower.Models.CharacterViewer.WeaponStats;

namespace watchtower.Models.Api {

    public class ExpandedWeaponStatEntry {

        public WeaponStatEntry Entry { get; set; } = new WeaponStatEntry();

        public PsCharacter? Character { get; set; }

    }
}
