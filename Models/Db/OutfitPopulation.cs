using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Models.Db {

    public class OutfitPopulation {

        public string OutfitID { get; set; } = "";

        public string? OutfitTag { get; set; }

        public string OutfitName { get; set; } = "";

        public short FactionID { get; set; }

        public int Count { get; set; }

    }
}
