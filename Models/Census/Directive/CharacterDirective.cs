using System;

namespace watchtower.Models.Census {

    public class CharacterDirective {

        public string CharacterID { get; set; } = "";

        public int DirectiveID { get; set; }

        public int TreeID { get; set; }

        public DateTime? CompletionDate { get; set; }

    }

}