using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Models.Api {

    /// <summary>
    ///     Represents the world population for a specific world at a specific time
    /// </summary>
    public class WorldPopulation {

        /// <summary>
        ///     ID of the world
        /// </summary>
        public short WorldID { get; set; }

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
        ///     How many NS we don't know the count of
        /// </summary>
        public int NsOther { get; set; }

    }
}
