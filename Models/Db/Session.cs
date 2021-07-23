using System;

namespace watchtower.Models.Db {

    public class Session {

        public string CharacterID { get; set; } = "";

        public DateTime Start { get; set; } = DateTime.UtcNow;

        public DateTime? End { get; set; }

    }

}