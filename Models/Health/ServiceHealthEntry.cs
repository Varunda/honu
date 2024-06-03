using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Models {

    /// <summary>
    ///     Represents meta information about a long running service 
    /// </summary>
    public class ServiceHealthEntry {

        /// <summary>
        ///     Name of the service this entry is for
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        ///     When this service was last ran to completion
        /// </summary>
        public DateTime LastRan { get; set; } = DateTime.UtcNow;

        /// <summary>
        ///     How many milliseconds it took for the service to be ran last time
        /// </summary>
        public long RunDuration { get; set; }

        /// <summary>
        ///     Is this service currently enabled and running? Only certain hosted services respect this
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        ///     A brief message about how the last service run went
        /// </summary>
        public string? Message { get; set; }

    }

}