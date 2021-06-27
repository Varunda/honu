using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Models.Events {

    /// <summary>
    /// 
    /// </summary>
    public class KillEvent {

        /// <summary>
        /// Character ID of the person who performed the kill
        /// </summary>
        public string AttackerCharacterID { get; set; } = "";

        /// <summary>
        /// Loadout of the attacker
        /// </summary>
        public short AttackerLoadoutID { get; set; }

        /// <summary>
        /// Fire mode
        /// </summary>
        public int AttackerFireModeID { get; set; }

        /// <summary>
        /// Vehicle ID
        /// </summary>
        public int AttackerVehicleID { get; set; }

        public string KilledCharacterID { get; set; } = "";

        public short KilledLoadoutID { get; set; }

        public short WorldID { get; set; }

        public int ZoneID { get; set; }

        public string WeaponID { get; set; } = "";

        public bool IsHeadshot { get; set; }

        public DateTime Timestamp { get; set; }

    }

}
