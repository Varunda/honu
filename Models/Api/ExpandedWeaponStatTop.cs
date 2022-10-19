using watchtower.Models.Census;
using watchtower.Models.CharacterViewer.WeaponStats;
using watchtower.Models.Db;

namespace watchtower.Models.Api {

    /// <summary>
    ///     Expanded to include the character of the stats
    /// </summary>
    public class ExpandedWeaponStatTop {

        /// <summary>
        ///     Weapon stat entry
        /// </summary>
        public WeaponStatTop Entry { get; set; } = new();

        /// <summary>
        ///     Character these stats are from
        /// </summary>
        public PsCharacter? Character { get; set; }

    }
}
