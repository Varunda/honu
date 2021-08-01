using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Models {

    public class TrackedConnection {

        public string ConnectionId { get; set; } = "";

        public short? WorldID { get; set; }

        public DateTime ConnectedAt { get; set; } = DateTime.UtcNow;

    }
}
