using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Code.Constants;

namespace watchtower.Models {

    public class SpawnEntries {

        /// <summary>
        ///     List of spawn entries 
        /// </summary>
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

        /// <summary>
        ///     What type of NPC is providing the spawns, such as sunderer, router, etc.
        /// </summary>
        public NpcType NpcType { get; set; } = NpcType.Unknown;

        /// <summary>
        ///     How many spawns in total this NPC has provided
        /// </summary>
        public int SpawnCount { get; set; }

        /// <summary>
        ///     When the first spawn was. Not when the NPC was pulled
        /// </summary>
        public DateTime FirstSeenAt { get; set; }

        /// <summary>
        ///     ID of the team/faction. Useful for NSO characters to tell what team they are providing spawns to
        /// </summary>
        public short FactionID { get; set; }

        /// <summary>
        ///     How many seconds alive this bus has been
        /// </summary>
        public int SecondsAlive { get; set; }

    }
}
