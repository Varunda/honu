using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Models {

    public class OutfitKillBlock {

        public List<TrackedOutfit> Entries = new List<TrackedOutfit>();

    }

    public class TrackedOutfit {

        public string ID { get; set; } = "";

        public string? Tag { get; set; }

        public string Name { get; set; } = "";

        public string FactionID { get; set; } = "";

        public int Kills { get; set; }

        public int Deaths { get; set; }

        public int Members { get; set; }

    }
}
