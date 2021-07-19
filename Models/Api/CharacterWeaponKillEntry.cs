using System;

namespace watchtower.Models.Api {

    public class CharacterWeaponKillEntry {

        public string WeaponID { get; set; } = "";

        public string WeaponName { get; set; } = "";

        public int Kills { get; set; }

        public int HeadshotKills { get; set; }

    }

}