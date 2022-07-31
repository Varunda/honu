using System;
using System.Collections.Generic;
using watchtower.Models.Census;
using watchtower.Models.Events;

namespace watchtower.Models.Api {

    /// <summary>
    ///     Represents a block of information about <see cref="ExpEvent"/>s that took place over a period of time.
    ///     This is meant to replace <see cref="ExpandedExpEvent"/>, as it contains tons of redundant information.
    ///     
    ///     For example, if a character healed another character twice, the character info would be duplicated
    /// </summary>
    public class ExperienceBlock {

        /// <summary>
        ///     Character that were used to create this block
        /// </summary>
        public List<string> InputCharacters { get; set; } = new List<string>();

        /// <summary>
        ///     When the events of this block start
        /// </summary>
        public DateTime PeriodStart { get; set; }

        /// <summary>
        ///     When the events of thie block end
        /// </summary>
        public DateTime PeriodEnd { get; set; }

        /// <summary>
        ///     List of all the characters that appear in the exp events, both the source and other
        /// </summary>
        public List<PsCharacter> Characters { get; set; } = new();

        /// <summary>
        ///     List of all the different types of exp events that happened
        /// </summary>
        public List<ExperienceType> ExperienceTypes { get; set; } = new();

        /// <summary>
        ///     Events themselves
        /// </summary>
        public List<ExpEvent> Events { get; set; } = new();

    }
}
