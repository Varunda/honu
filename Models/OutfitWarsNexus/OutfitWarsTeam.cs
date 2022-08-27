using System.Collections.Generic;
using watchtower.Models.Events;

namespace watchtower.Models.OutfitWarsNexus {

    /// <summary>
    ///     Represents the information about a team in an outfit wars match
    /// </summary>
    public class OutfitWarsTeam {

        /// <summary>
        ///     ID of the outfit
        /// </summary>
        public string OutfitID { get; set; } = "";

        /// <summary>
        ///     Members in this team
        /// </summary>
        public HashSet<string> Members { get; set; } = new();

        /// <summary>
        ///     Team ID of the outfit, which may be different than the faction ID
        /// </summary>
        public short TeamID { get; set; }

        /// <summary>
        ///     How many kills this team has gotten
        /// </summary>
        public List<KillEvent> Kills { get; set; } = new();

        /// <summary>
        ///     How many deaths this team has had
        /// </summary>
        public List<KillEvent> Deaths { get; set; } = new();

        /// <summary>
        ///     How many revives this team has done
        /// </summary>
        public List<ExpEvent> Experience { get; set; } = new();

        /// <summary>
        ///     How many vehicles have be killed by members of this outfit
        /// </summary>
        public List<VehicleDestroyEvent> VehicleKills { get; set; } = new();

    }
}
