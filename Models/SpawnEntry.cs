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

        public string Owner { get; set; } = "";

        public NpcType NpcType { get; set; } = NpcType.Unknown;

        public int SpawnCount { get; set; }

        public DateTime FirstSeenAt { get; set; }

        public short FactionID { get; set; }

        public int SecondsAlive { get; set; }

    }
}
