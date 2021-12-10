using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models.Db;

namespace watchtower.Models.Api {

    /// <summary>
    ///     Grouped collection of all the percentile stats for weapon usage
    /// </summary>
    public class WeaponStatPercentileAll {

        /// <summary>
        ///     ID of the item
        /// </summary>
        public string ItemID { get; set; } = "";

        /// <summary>
        ///     Buckets that contain accuracy intervals
        /// </summary>
        public List<Bucket> Accuracy { get; set; } = new List<Bucket>();

        /// <summary>
        ///     Buckets that contain headshot ratio intervals
        /// </summary>
        public List<Bucket> HeadshotRatio { get; set; } = new List<Bucket>();

        /// <summary>
        ///     Buckets that contain KD intervals
        /// </summary>
        public List<Bucket> KD { get; set; } = new List<Bucket>();

        /// <summary>
        ///     Buckets that contain KPM intervals
        /// </summary>
        public List<Bucket> KPM { get; set; } = new List<Bucket>();

    }
}
