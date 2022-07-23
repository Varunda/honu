using System;

namespace watchtower.Models.Api {

    public class OutfitActivity {

        /// <summary>
        ///     ID of the outfit
        /// </summary>
        public string? OutfitID { get; set; } = null;

        /// <summary>
        ///     When the start of this period is
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        ///     How many seconds this interval covers
        /// </summary>
        public int IntervalSeconds { get; set; }

        /// <summary>
        ///     How many members were online at this time
        /// </summary>
        public int Count { get; set; }

    }
}
