using System;
using System.Collections.Generic;
using watchtower.Models.Events;

namespace watchtower.Models.RealtimeAlert {

    /// <summary>
    ///     Represents the information about a team in an outfit wars match
    /// </summary>
    public class RealtimeAlertTeam {

        public RealtimeAlertTeam() {

        }

        public RealtimeAlertTeam(RealtimeAlertTeam other) {
            TeamID = other.TeamID;
            Kills = other.Kills;
            Deaths = other.Deaths;
            Experience = new Dictionary<int, int>(other.Experience);
            VehicleKills = other.VehicleKills;
            VehicleDeaths = other.VehicleDeaths;
        }

        /// <summary>
        ///     Team ID of the outfit, which may be different than the faction ID
        /// </summary>
        public short TeamID { get; set; }

        /// <summary>
        ///     How many kills this team has gotten
        /// </summary>
        public int Kills { get; set; } 

        /// <summary>
        ///     How many deaths this team has had
        /// </summary>
        public int Deaths { get; set; } 

        /// <summary>
        ///     Experience IDs and how many ticks
        /// </summary>
        public Dictionary<int, int> Experience { get; set; } = new(); // <experience ID, times ticked>

        /// <summary>
        ///     How many vehicles have be killed by members of this outfit
        /// </summary>
        public int VehicleKills { get; set; }

        /// <summary>
        ///     How many vehicles have died
        /// </summary>
        public int VehicleDeaths { get; set; }

        public List<KillEvent> KillDeathEvents { get; } = new();

        public List<ExpEvent> ExpEvents { get; } = new();

        public List<VehicleDestroyEvent> VehicleDestroyEvents { get; } = new();

    }
}
