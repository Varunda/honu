using System;

namespace watchtower.Models.Health {

    public class BadHealthEntry {

        public DateTime When { get; set; }

        public string What { get; set; } = "";

    }
}
