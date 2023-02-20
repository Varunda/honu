using System;

namespace watchtower.Models.Db {

    /// <summary>
    ///     Information about when a continent last locked
    /// </summary>
    public class ContinentLockEntry {

        /// <summary>
        ///     ID of the zone, unique per world
        /// </summary>
        public uint ZoneID { get; set; }

        /// <summary>
        ///     ID of the world
        /// </summary>
        public short WorldID { get; set; }

        /// <summary>
        ///     When this continent was last locked
        /// </summary>
        public DateTime Timestamp { get; set; }

    }
}
