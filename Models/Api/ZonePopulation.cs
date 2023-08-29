using System;

namespace watchtower.Models.Api {

    /// <summary>
    ///     Summary of a zone's population in a world
    /// </summary>
    public class ZonePopulation {

        /// <summary>
        ///     ID of the world
        /// </summary>
        public short WorldID { get; set; }

        /// <summary>
        ///     ID of the zone this data is for
        /// </summary>
        public uint ZoneID { get; set; }

        /// <summary>
        ///     When this data was generated
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        ///     When returned in an API response, how long it will stay in the cache before being evicted
        /// </summary>
        public DateTime CachedUntil { get; set; }

        /// <summary>
        ///     How many in total are in the world
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        ///     How many VS are online
        /// </summary>
        public int Vs { get; set; }

        /// <summary>
        ///     How many NC are online
        /// </summary>
        public int Nc { get; set; }

        /// <summary>
        ///     How many TR are online
        /// </summary>
        public int Tr { get; set; }

        /// <summary>
        ///     How many NS are online
        /// </summary>
        public int Ns { get; set; }

        /// <summary>
        ///     How many NS are VS 
        /// </summary>
        public int Ns_vs { get; set; }

        /// <summary>
        ///     How many NS are NC
        /// </summary>
        public int Ns_nc { get; set; }

        /// <summary>
        ///     How many NS are TR
        /// </summary>
        public int Ns_tr { get; set; }

        /// <summary>
        ///     How many NS haven't done anything to be assigned a team yet. This is a tracking issue,
        ///     not a limitation of the ps2 API
        /// </summary>
        public int NsOther { get; set; }

    }
}
