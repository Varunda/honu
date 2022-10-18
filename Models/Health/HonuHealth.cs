using System;
using System.Collections.Generic;
using watchtower.Models.Api;

namespace watchtower.Models.Health {

    /// <summary>
    ///     Information about the health of Honu
    /// </summary>
    public class HonuHealth {

        /// <summary>
        ///     Information about the realtime Death stream
        /// </summary>
        public List<CensusRealtimeHealthEntry> Death { get; set; } = new List<CensusRealtimeHealthEntry>();

        /// <summary>
        ///     Information about the realtime Exp stream
        /// </summary>
        public List<CensusRealtimeHealthEntry> Exp { get; set; } = new List<CensusRealtimeHealthEntry>();

        /// <summary>
        ///     Information about when Honu last had a bad realtime stream
        /// </summary>
        public List<BadHealthEntry> RealtimeHealthFailures { get; set; } = new List<BadHealthEntry>();

        /// <summary>
        ///     Reconnects in the last 24 hours
        /// </summary>
        public List<RealtimeReconnectEntry> Reconnects { get; set; } = new List<RealtimeReconnectEntry>();

        /// <summary>
        ///     Information about the hosted queues in Honu
        /// </summary>
        public List<ServiceQueueCount> Queues { get; set; } = new List<ServiceQueueCount>();

        /// <summary>
        ///     When this data was created
        /// </summary>
        public DateTime Timestamp { get; set; }

    }
}
