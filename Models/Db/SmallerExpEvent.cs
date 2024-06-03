using System;
using watchtower.Models.Events;
using watchtower.Services.Hosted;

namespace watchtower.Models.Db {

    /// <summary>
    ///     this is a smaller <see cref="ExpEvent"/> that is only used in <see cref="HostedSessionSummaryProcess"/> (update this if wrong)
    ///     to prevent loading 750MB of just exp events (only 350MB instead)
    /// </summary>
    public class SmallerExpEvent {

        public ulong SourceID { get; set; } 

        public int ExperienceID { get; set; }

        public int Amount { get; set; }

        public DateTime Timestamp { get; set; }

    }
}
