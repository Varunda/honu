using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Models.Census {

    public class PsMapHex {

        public int RegionID { get; set; }

        public uint ZoneID { get; set; }

        public int X { get; set; }

        public int Y { get; set; }

        public int HexType { get; set; }

    }
}
