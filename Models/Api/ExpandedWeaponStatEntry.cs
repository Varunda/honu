using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models.Census;
using watchtower.Models.CharacterViewer.WeaponStats;

namespace watchtower.Models.Api {

    /// <summary>
    ///     Wrapper around a <see cref="WeaponStatEntry"/> that also includes the <see cref="PsCharacter"/> it's for
    /// </summary>
    public class ExpandedWeaponStatEntry {

        /// <summary>
        ///     Wrapped entry
        /// </summary>
        public WeaponStatEntry Entry { get; set; } = new WeaponStatEntry();

        /// <summary>
        ///     Character the <see cref="WeaponStatEntry"/> is for, using the <see cref="WeaponStatEntry.CharacterID"/>
        ///     of <see cref="Entry"/>. <c>null</c> if the character does not exist
        /// </summary>
        public PsCharacter? Character { get; set; }

    }
}
