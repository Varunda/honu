using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Models.Census {

    public class PsOutfit {

        public string ID { get; set; } = "";

        public string Name { get; set; } = "";

        public string? Tag { get; set; }

        public short FactionID { get; set; }

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    }
}
