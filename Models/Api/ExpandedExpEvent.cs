using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models.Census;
using watchtower.Models.Events;

namespace watchtower.Models.Api {

    /// <summary>
    ///     Wrapper around an exp event that contains more information
    /// </summary>
    public class ExpandedExpEvent {

        /// <summary>
        ///     Exp event this expanded event is for
        /// </summary>
        public ExpEvent Event { get; set; } = new ExpEvent();

        /// <summary>
        ///     Character that produced the event
        /// </summary>
        public PsCharacter? Source { get; set; }

        /// <summary>
        ///     If the other_id of the event is a character ID, this will contain the character that is in other_id
        /// </summary>
        public PsCharacter? Other { get; set; }

    }
}
