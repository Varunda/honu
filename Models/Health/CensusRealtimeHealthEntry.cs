using Newtonsoft.Json.Linq;
using System;

namespace watchtower.Models {

    /// <summary>
    ///     Represents the health of a realtime census event stream
    /// </summary>
    public class CensusRealtimeHealthEntry {

        /// <summary>
        ///     ID of the world this event stream is from
        /// </summary>
        public short WorldID { get; set; }

        /// <summary>
        ///     When this stream received it's first event
        /// </summary>
        public DateTime? FirstEvent { get; set; }

        /// <summary>
        ///     When an event was last received on this stream
        /// </summary>
        public DateTime? LastEvent { get; set; }

        /// <summary>
        ///     json of the last event parsed
        /// </summary>
        public JToken? LastEventData { get; set; }

        /// <summary>
        ///     How many times the health check has failed for this event stream
        /// </summary>
        public int FailureCount { get; set; }

        /// <summary>
        ///     How many events have been receieved on this current connection
        /// </summary>
        public int EventCount { get; set; }

    }
}
