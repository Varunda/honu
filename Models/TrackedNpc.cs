using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Models {

    public class TrackedNpc {

        public string NpcID { get; set; } = "";

        public string Type { get; set; } = "";

        public string OwnerID { get; set; } = "";

        public int SpawnCount { get; set; }

        public DateTime FirstSeenAt { get; set; } = DateTime.UtcNow;

        public int LatestEventAt { get; set; }

    }
}
