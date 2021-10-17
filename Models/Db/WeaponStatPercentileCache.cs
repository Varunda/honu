using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Models.Db {

    public class WeaponStatPercentileCache {

        /// <summary>
        /// ID of the item
        /// </summary>
        public string ItemID { get; set; } = "";

        /// <summary>
        /// What type of percentile cache is this data for?
        /// </summary>
        public short TypeID { get; set; }

        /// <summary>
        /// When was this data generated?
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Is the percentile data loaded? 
        /// </summary>
        public bool Loaded { get; set; } = false;

        public double Q0 { get; set; }

        public double Q5 { get; set; }

        public double Q10 { get; set; }

        public double Q15 { get; set; }

        public double Q20 { get; set; }

        public double Q25 { get; set; }

        public double Q30 { get; set; }

        public double Q35 { get; set; }

        public double Q40 { get; set; }

        public double Q45 { get; set; }

        public double Q50 { get; set; }

        public double Q55 { get; set; }

        public double Q60 { get; set; }

        public double Q65 { get; set; }

        public double Q70 { get; set; }

        public double Q75 { get; set; }

        public double Q80 { get; set; }

        public double Q85 { get; set; }

        public double Q90 { get; set; }

        public double Q95 { get; set; }

        public double Q100 { get; set; }

    }
}
