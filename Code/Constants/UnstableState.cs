using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Code.Constants {

    /// <summary>
    /// Represents the possible states of a zone
    /// </summary>
    public enum UnstableState {

        /// <summary>
        /// The zone is locked
        /// </summary>
        LOCKED = 0,

        /// <summary>
        /// There is a single lane opened
        /// </summary>
        SINGLE_LANE = 1,

        /// <summary>
        /// There are two lanes opened
        /// </summary>
        DOUBLE_LANE = 2,

        /// <summary>
        /// The zone is fully opened
        /// </summary>
        UNLOCKED = 3

    }
}
