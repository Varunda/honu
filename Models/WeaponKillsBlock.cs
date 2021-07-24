using System;
using System.Collections.Generic;

namespace watchtower.Models {

    public class WeaponKillsBlock {

        public List<WeaponKillEntry> Entries { get; set; } = new List<WeaponKillEntry>();

    }

    public class WeaponKillEntry {

        public string ItemID { get; set; } = "";

        public string ItemName { get; set; } = "";

        public int Kills { get; set; }

    }

}