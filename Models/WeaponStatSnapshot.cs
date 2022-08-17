using System;

namespace watchtower.Models {

    public class WeaponStatSnapshot {

        /// <summary>
        ///     Unique ID of the snapshot
        /// </summary>
        public long ID { get; set; }

        /// <summary>
        ///     ID of the item
        /// </summary>
        public int ItemID { get; set; }

        /// <summary>
        ///     When this snapshot was created
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        ///     How many unique users have used this gun
        /// </summary>
        public int Users { get; set; }

        public long Kills { get; set; }

        public long Deaths { get; set; }

        public long Shots { get; set; }

        public long ShotsHit { get; set; }

        public long Headshots { get; set; }

        public long VehicleKills { get; set; }

        public long SecondsWith { get; set; }

    }
}
