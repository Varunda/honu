using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Models.CharacterViewer.WeaponStats {

    public class WeaponStat {

        public string CharacterID { get; set; } = "";

        public string StatName { get; set; } = "";

        public string ItemID { get; set; } = "";

        public string VehicleID { get; set; } = "";

        public int Value { get; set; }

    }
}
