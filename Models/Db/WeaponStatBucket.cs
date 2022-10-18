using System;
using watchtower.Code.Constants;

namespace watchtower.Models.Db {

    /// <summary>
    ///     Bucketed weapon stats
    /// </summary>
    public class WeaponStatBucket {

        /// <summary>
        ///     Unique ID of the bucket
        /// </summary>
        public long ID { get; set; }

        /// <summary>
        ///     Item ID this bucket is for
        /// </summary>
        public int ItemID { get; set; }

        /// <summary>
        ///     What type is this bucket for, see <see cref="PercentileCacheType"/>
        /// </summary>
        public short TypeID { get; set; }

        /// <summary>
        ///     When this bucket was created
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        ///     What value this bucket starts at
        /// </summary>
        public double Start { get; set; }

        /// <summary>
        ///     How width this bucket is
        /// </summary>
        public double Width { get; set; }

        /// <summary>
        ///     How many entries exist in this bucket
        /// </summary>
        public int Count { get; set; }

    }
}
