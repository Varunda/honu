using System;

namespace watchtower.Models.Api {

    /// <summary>
    ///     Represents the amount of kills a character has gotten
    /// </summary>
    public class CharacterWeaponKillEntry {

        /// <summary>
        ///     ID of the item
        /// </summary>
        public string WeaponID { get; set; } = "";

        /// <summary>
        ///     Name of the item
        /// </summary>
        public string WeaponName { get; set; } = "";

        /// <summary>
        ///     How many kills have been gotten
        /// </summary>
        public int Kills { get; set; }

        /// <summary>
        ///     How many kills have been from headshots
        /// </summary>
        public int HeadshotKills { get; set; }

    }

}