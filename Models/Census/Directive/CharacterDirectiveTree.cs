
using System;

namespace watchtower.Models.Census {

    public class CharacterDirectiveTree {

        public string CharacterID { get; set; } = "";

        public int TreeID { get; set; }

        public int CurrentTier { get; set; }

        public int CurrentLevel { get; set; }

        public DateTime? CompletionDate { get; set; }

    }

}