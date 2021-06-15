using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Models {

    public class TrackedPlayer {

        /// <summary>
        /// Character ID of a player
        /// </summary>
        public string ID { get; set; } = "";

        /// <summary>
        /// Faction the character is on
        /// </summary>
        public string FactionID { get; set; } = "";

        /// <summary>
        /// ID of the server/world the player is on
        /// </summary>
        public string WorldID { get; set; } = "";

        /// <summary>
        /// If the character is online or not
        /// </summary>
        public bool Online { get; set; }

        /// <summary>
        /// List of timestamps of when a player gets a kill
        /// </summary>
        public List<int> Kills { get; set; } = new List<int>();

        /// <summary>
        /// List of timestamps of when a player is killed
        /// </summary>
        public List<int> Deaths { get; set; } = new List<int>();

        /// <summary>
        /// List of timestamps of when a player gets an assist
        /// </summary>
        public List<int> Assists { get; set; } = new List<int>();

        /// <summary>
        /// List of timestamps of when a player gets heal experience
        /// </summary>
        public List<int> Heals { get; set; } = new List<int>();

        /// <summary>
        /// List of timestamps of when a player gets revive experience
        /// </summary>
        public List<int> Revives { get; set; } = new List<int>();

        /// <summary>
        /// List of timestamps of when a player gets resupply experience
        /// </summary>
        public List<int> Resupplies { get; set; } = new List<int>();

        /// <summary>
        /// List of timestamps of when a player gets repair experience
        /// </summary>
        public List<int> Repairs { get; set; } = new List<int>();

        /// <summary>
        /// List of timestamps of when a player gets spawn experience, such as sundy and beacon spawns
        /// </summary>
        public List<int> Spawns { get; set; } = new List<int>();

    }
}
