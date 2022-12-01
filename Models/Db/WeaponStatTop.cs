using System;
using watchtower.Code.Constants;
using watchtower.Models.Census;
using watchtower.Models.CharacterViewer.WeaponStats;

namespace watchtower.Models.Db {

    /// <summary>
    ///     Represents a copy of the top characters with a weapon, which also has a copy of the
    ///     information from <see cref="WeaponStatEntry"/>
    /// </summary>
    public class WeaponStatTop {

        /// <summary>
        ///     Unique ID of the entry
        /// </summary>
        public long ID { get; set; }

        /// <summary>
        ///     What world the character with <see cref="CharacterID"/> is on
        /// </summary>
        public short WorldID { get; set; }

        /// <summary>
        ///     What faction (not team) the character with <see cref="CharacterID"/> is on
        /// </summary>
        public short FactionID { get; set; }

        /// <summary>
        ///     Matches to <see cref="PercentileCacheType"/>
        /// </summary>
        public short TypeID { get; set; }

        /// <summary>
        ///     When this entry was created
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        ///     ID of the <see cref="PsCharacter"/> this entry is for
        /// </summary>
        public string CharacterID { get; set; } = "";

        public int ItemID { get; set; }

        public int VehicleID { get; set; }

        public int Kills { get; set; }

        public int Deaths { get; set; }

        public int Headshots { get; set; }

        public int Shots { get; set; }

        public int ShotsHit { get; set; }

        public int SecondsWith { get; set; }

        public int VehicleKills { get; set; }

        public double Accuracy { get; set; }

        public double HeadshotRatio { get; set; }

        public double KillDeathRatio { get; set; }

        public double KillsPerMinute { get; set; }

        public double VehicleKillsPerMinute { get; set; }

    }
}
