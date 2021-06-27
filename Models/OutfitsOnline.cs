using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Models {

    public class OutfitsOnline {

        public List<OutfitOnlineEntry> Outfits { get; set; } = new List<OutfitOnlineEntry>();

        public int TotalOnline { get; set; }

    }

    public class OutfitOnlineEntry {

        public string Display { get; set; } = "";

        public int AmountOnline { get; set; }

        public string FactionID { get; set; } = "";

    }
}
