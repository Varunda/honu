using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Models {

    /// <summary>
    /// Represents a pair of unix second timestamps
    /// </summary>
    public struct TimestampPair {

        /// <summary>
        /// First value, name is some what arbitrary
        /// </summary>
        public long Start { get; set; }

        /// <summary>
        /// Second value, name is some what arbitrary
        /// </summary>
        public long End { get; set; }

    }
}
