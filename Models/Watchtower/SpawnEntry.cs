using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Code.Constants;

namespace watchtower.Models {

    public class SpawnEntries {

        public List<SpawnEntry> Entries { get; set; } = new List<SpawnEntry>();

    }

    public class SpawnEntry {

        /// <summary>
        ///     ID of the character who this spawn entry is for
        /// </summary>
        public string OwnerID { get; set; } = "";

        /// <summary>
        ///     Display string of the owner
        /// </summary>
        public string Owner { get; set; } = "";

        public NpcType NpcType { get; set; } = NpcType.Unknown;

        public int SpawnCount { get; set; }

        public DateTime FirstSeenAt { get; set; }

        public short FactionID { get; set; }

        public int SecondsAlive { get; set; }

    }
}
