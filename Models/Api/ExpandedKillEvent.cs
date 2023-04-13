using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models.Census;
using watchtower.Models.Events;

namespace watchtower.Models.Api {

    /// <summary>
    ///     a <see cref="KillEvent"/> with extra information
    /// </summary>
    public class ExpandedKillEvent {

        /// <summary>
        ///     original event
        /// </summary>
        public KillEvent Event { get; set; } = new KillEvent();

        /// <summary>
        ///     character of the <see cref="KillEvent.AttackerCharacterID"/>
        /// </summary>
        public PsCharacter? Attacker { get; set; }

        /// <summary>
        ///     character of the <see cref="KillEvent.KilledCharacterID"/>
        /// </summary>
        public PsCharacter? Killed { get; set; }

        /// <summary>
        ///     weapon of the <see cref="KillEvent.WeaponID"/>
        /// </summary>
        public PsItem? Item { get; set; }

        /// <summary>
        ///     fire mode of the <see cref="KillEvent.AttackerFireModeID"/>
        /// </summary>
        public FireGroupToFireMode? FireGroupToFireMode { get; set; }

    }
}
