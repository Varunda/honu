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

        public override string ToString() {
            return $"<{nameof(WeaponStatPercentileCache)}: [ItemID={ItemID}] [Timestamp={Timestamp:u}] [Loaded={Loaded}]"
                + $" [Q0={Q0}] [Q5={Q5}] [Q10={Q10}] [Q15={Q15}] [Q20={Q20}] [Q25={Q25}] [Q30={Q30}] [Q35={Q35}] [Q40={Q40}] [Q45={Q45}]"
                + $" [Q50={Q50}] [Q55={Q55}] [Q60={Q60}] [Q65={Q65}] [Q70={Q70}] [Q75={Q75}] [Q80={Q80}] [Q85={Q85}] [Q90={Q90}] [Q95={Q95}] [Q100={Q100}]>";
        }


    }
}
