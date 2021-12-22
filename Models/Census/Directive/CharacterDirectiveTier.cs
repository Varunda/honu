using System;

namespace watchtower.Models.Census {

    public class CharacterDirectiveTier {

        public string CharacterID { get; set; } = "";

        public int TreeID { get; set; }

        public int TierID { get; set; }

        public DateTime? CompletionDate { get; set; }

    }

}