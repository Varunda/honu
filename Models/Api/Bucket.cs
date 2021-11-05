using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Models.Api {

    /// <summary>
    ///     A single bucket for the empirical distribution of a discrete dataset
    /// </summary>
    public class Bucket {

        /// <summary>
        ///     Where this bucket starts
        /// </summary>
        public double Start { get; set; } = 0d;

        /// <summary>
        ///     How wide this bucket is
        /// </summary>
        public double Width { get; set; }

        /// <summary>
        ///     How many values are in this bucket
        /// </summary>
        public int Count { get; set; }

    }

}
