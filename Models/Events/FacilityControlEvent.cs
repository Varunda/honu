using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Models.Events {

    public class FacilityControlEvent {

        public int FacilityID { get; set; }

        public DateTime Timestamp { get; set; }

        public int DurationHeld { get; set; }

        public int Players { get; set; }

        public short NewFactionID { get; set; }

        public short OldFactionID { get; set; }

        public string? OutfitID { get; set; }

        public short WorldID { get; set; }

        public uint ZoneID { get; set; }

    }
}
