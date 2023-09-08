using System.Collections.Generic;
using watchtower.Models.Census;

namespace watchtower.Models.Api {

    public class ExpandedKillboardEntry {

        /// <summary>
        ///     Entry that has extra info being provided about it
        /// </summary>
        public KillboardEntry Entry { get; set; } = new();

        /// <summary>
        ///     Character from <see cref="KillboardEntry.OtherCharacterID"/>
        /// </summary>
        public PsCharacter? Character { get; set; }

        /// <summary>
        ///     Optional list of character stats for the character
        /// </summary>
        public List<PsCharacterHistoryStat>? Stats { get; set; } = null;

    }
}
