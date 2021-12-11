using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Models {

    /// <summary>
    ///     A brief overview of what is going on in a world/server
    /// </summary>
    public class WorldOverview {

        /// <summary>
        ///     ID of the world
        /// </summary>
        public short WorldID { get; set; }

        /// <summary>
        ///     Name of the world
        /// </summary>
        public string WorldName { get; set; } = "";

        /// <summary>
        ///     How many players are online
        /// </summary>
        public int PlayersOnline { get; set; }

        /// <summary>
        ///     The state of each zone
        /// </summary>
        public List<ZoneState> Zones { get; set; } = new List<ZoneState>();

    }
}
