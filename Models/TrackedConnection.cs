using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Models {

    /// <summary>
    ///     Metadata about a connection to the realtime data hub
    /// </summary>
    public class TrackedConnection {

        /// <summary>
        ///     ID of the connection
        /// </summary>
        public string ConnectionId { get; set; } = "";

        /// <summary>
        ///     What ID the client has requested ata for
        /// </summary>
        public short? WorldID { get; set; }

        /// <summary>
        ///     How many minutes back does this client want the info for?
        /// </summary>
        public int Duration { get; set; }

        /// <summary>
        ///     When the client first connected
        /// </summary>
        public DateTime ConnectedAt { get; set; } = DateTime.UtcNow;

    }
}
