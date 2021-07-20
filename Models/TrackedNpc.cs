using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Code.Constants;

namespace watchtower.Models {

    public class TrackedNpc {

        public string NpcID { get; set; } = "";

        public NpcType Type { get; set; } = NpcType.Unknown;

        public string OwnerID { get; set; } = "";

        public int SpawnCount { get; set; }

        public DateTime FirstSeenAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Milliseconds
        /// </summary>
        public long LatestEventAt { get; set; }

        public short WorldID { get; set; }

    }
}
