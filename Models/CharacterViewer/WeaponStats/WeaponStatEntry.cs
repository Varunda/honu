using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Models.CharacterViewer.WeaponStats {

    public class WeaponStatEntry {

        public string WeaponID { get; set; } = "";

        public string CharacterID { get; set; } = "";

        public int Kills { get; set; }

        public int Deaths { get; set; }

        public int Headshots { get; set; }

        public int Shots { get; set; }

        public int ShotsHit { get; set; }

        public int SecondsWith { get; set; }

        public int VehicleKills { get; set; }

        /// <summary>
        /// When the entry was created
        /// </summary>
        public DateTime Timestamp { get; set; }

        public double Accuracy { get; set; }

        public double HeadshotRatio { get; set; }

        public double KillDeathRatio { get; set; }

        public double KillsPerMinute { get; set; }

        public double VehicleKillsPerMinute { get; set; }

    }
}
