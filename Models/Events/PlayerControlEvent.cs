using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Models.Events {

    public class PlayerControlEvent {

        public bool IsCapture { get; set; }

        public string CharacterID { get; set; } = "";

        public string? OutfitID { get; set; }

        public int FacilityID { get; set; }

        public DateTime Timestamp { get; set; }

        public short WorldID { get; set; }

        public uint ZoneID { get; set; }

    }

}
