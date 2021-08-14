
namespace watchtower.Models.Db {

    /// <summary>
    /// Used for the top weapon killboards
    /// </summary>
    public class KillItemEntry {

        /// <summary>
        /// ID of the weapon
        /// </summary>
        public string ItemID { get; set; } = "";

        /// <summary>
        /// How many kills the weapon has gotten
        /// </summary>
        public int Kills { get; set; }

        /// <summary>
        /// How many headshot kills the weapon has gotten
        /// </summary>
        public int HeadshotKills { get; set; }

        /// <summary>
        /// How many unique people have used the weapon
        /// </summary>
        public int Users { get; set; }

    }

}