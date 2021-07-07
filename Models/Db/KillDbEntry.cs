using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Models.Db {

    public class KillDbEntry {

        public string CharacterID { get; set; } = "";

        public int Kills { get; set; }

        public int Deaths { get; set; }

        public int Assist { get; set; }

    }
}
