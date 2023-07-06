using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models.Census;
using watchtower.Models.CharacterViewer.WeaponStats;

namespace watchtower.Models.Api {

    public class CharacterWeaponStatEntry {

        public string CharacterID { get; set; } = "";

        public string ItemID { get; set; } = "";

        public PsItem? Item { get; set; }

        public PsVehicle? Vehicle { get; set; }

        public WeaponStatEntry Stat { get; set; } = new WeaponStatEntry();

        public double? KillDeathRatioPercentile { get; set; }

        public double? KillsPerMinutePercentile { get; set; }

        public double? AccuracyPercentile { get; set; }

        public double? HeadshotRatioPercentile { get; set; }

        public double? VehicleKillsPerMinutePercentile { get; set; }

    }

}
