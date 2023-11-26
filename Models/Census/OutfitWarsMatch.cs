using System;

namespace watchtower.Models.Census {

    public class OutfitWarsMatch {

        public string MatchID { get; set; } = "";

        public string RoundID { get; set; } = "";

        public int OutfitWarID { get; set; }

        public string OutfitAId { get; set; } = "";

        public string OutfitBId { get; set; } = "";

        public DateTime Timestamp { get; set; }

        public short WorldID { get; set; }

        public short OutfitAFactionId { get; set; }

        public short OutfitBFactionId { get; set; }

    }
}
