using System;

namespace watchtower.Models.Health {

    public class RealtimeReconnectEntry {

        /// <summary>
        ///     Unique ID for the DB
        /// </summary>
        public long ID { get; set; }

        /// <summary>
        ///     When this reconnect took place
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        ///     ID of the world this reconnect was triggered by
        /// </summary>
        public short WorldID { get; set; }

        /// <summary>
        ///     What stream of events was being checked for
        /// </summary>
        public string StreamType { get; set; } = "";

        /// <summary>
        ///     How many times the health check failed (and how many times Honu reconnected), before an event
        ///     was found
        /// </summary>
        public int FailedCount { get; set; }

        /// <summary>
        ///     How many seconds between the last event and when this new event was received
        /// </summary>
        public int Duration { get; set; }

        /// <summary>
        ///     How many events the previous stream got before it was reset
        /// </summary>
        public int EventCount { get; set; }

    }
}
