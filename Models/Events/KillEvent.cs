using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Models.Events {

    /// <summary>
    ///     Represents data about a Death eventfrom the realtime socket
    /// </summary>
    public class KillEvent {

        /// <summary>
        ///     ID of the kill event
        /// </summary>
        public long ID { get; set; }

        /// <summary>
        ///     Character ID of the person who performed the kill
        /// </summary>
        public string AttackerCharacterID { get; set; } = "";

        /// <summary>
        ///     Loadout of the attacker
        /// </summary>
        public short AttackerLoadoutID { get; set; }

        /// <summary>
        ///     What team the attacker was on when the kill happened
        /// </summary>
        public short AttackerTeamID { get; set; }

        /// <summary>
        ///     Fire mode of the attacker
        /// </summary>
        public int AttackerFireModeID { get; set; }

        /// <summary>
        ///     Vehicle ID of the attacker
        /// </summary>
        public int AttackerVehicleID { get; set; }

        /// <summary>
        ///     ID of the character who was killed
        /// </summary>
        public string KilledCharacterID { get; set; } = "";

        /// <summary>
        ///     Loadout ID of the character that was killed
        /// </summary>
        public short KilledLoadoutID { get; set; }

        /// <summary>
        ///     Team the killed character was on
        /// </summary>
        public short KilledTeamID { get; set; }

        /// <summary>
        ///     World the kill happened on
        /// </summary>
        public short WorldID { get; set; }

        /// <summary>
        ///     Zone the kill happened on
        /// </summary>
        public uint ZoneID { get; set; }

        /// <summary>
        ///     Weapon used by the attacker
        /// </summary>
        public string WeaponID { get; set; } = "";

        /// <summary>
        ///     If the kill waas a headshot kill or not
        /// </summary>
        public bool IsHeadshot { get; set; }

        /// <summary>
        ///     ID of the exp event that was used to revive the character
        /// </summary>
        public long? RevivedEventID { get; set; }

        /// <summary>
        ///     When the kill happened
        /// </summary>
        public DateTime Timestamp { get; set; }

    }

}
