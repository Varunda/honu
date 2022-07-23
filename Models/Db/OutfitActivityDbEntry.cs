using System;

namespace watchtower.Models.Db {

    public class OutfitActivityDbEntry {

        /// <summary>
        ///     Timestamp of when this time range started
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        ///     How many unique characters were online during this period
        /// </summary>
        public int Count { get; set; }

    }

}
