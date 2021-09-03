using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Models.Census {

    public class PsFacility {

        public int FacilityID { get; set; }

        public uint ZoneID { get; set; }

        public int RegionID { get; set; }

        public string Name { get; set; } = "";

        public int TypeID { get; set; }

        public string TypeName { get; set; } = "";

        public decimal? LocationX { get; set; }

        public decimal? LocationY { get; set; }

        public decimal? LocationZ { get; set; }
        
    }
}
