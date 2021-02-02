using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Models {

    public class Character {

        public string ID { get; set; } = "";

        public string Name { get; set; } = "";

        public bool Online { get; set; }

        public string WorldID { get; set; } = "";

        public string FactionID { get; set; } = "";

        public string? OutfitID { get; set; }

        public string? OutfitTag { get; set; }

        public string? OutfitName { get; set; }

    }
}
