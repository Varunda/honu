using System;

namespace watchtower.Models.Db {

    public class WeaponStatTop {

        public long ID { get; set; }

        public short WorldID { get; set; }

        public short FactionID { get; set; }

        public short TypeID { get; set; }

        public DateTime Timestamp { get; set; }

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
