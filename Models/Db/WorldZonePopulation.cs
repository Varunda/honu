using System;

namespace watchtower.Models.Db {

    /// <summary>
    ///     Contains a snapshot of the world population at a specific time
    /// </summary>
    public class WorldZonePopulation {

        /// <summary>
        ///     ID of the world
        /// </summary>
        public short WorldID { get; set; }

        /// <summary>
        ///     ID of the zone. Only the main 5 are calculated
        /// </summary>
        public uint ZoneID { get; set; }

        /// <summary>
        ///     When this snapshot was taken
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        ///     How many total players are online 
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        ///     How many players from the VS faction are online
        /// </summary>
        public int FactionVs { get; set; }

        /// <summary>
        ///     How many players from the NC faction are online
        /// </summary>
        public int FactionNc { get; set; }

        /// <summary>
        ///     How many players from the TR faction are online
        /// </summary>
        public int FactionTr { get; set; }

        /// <summary>
        ///     How many players from the NS faction are online
        /// </summary>
        public int FactionNs { get; set; }

        /// <summary>
        ///     How many players are currently playing for VS (VS faction + NS)
        /// </summary>
        public int TeamVs { get; set; }

        /// <summary>
        ///     How many players are currently playing for NC (NC faction + NS)
        /// </summary>
        public int TeamNc { get; set; }

        /// <summary>
        ///     How many players are currently playing for TR (TR faction + NS)
        /// </summary>
        public int TeamTr { get; set; }

        /// <summary>
        ///     How many players are on a faction that is not known. Usually NS without an event
        /// </summary>
        public int TeamUnknown { get; set; }

    }
}
