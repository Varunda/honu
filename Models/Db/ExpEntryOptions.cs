using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Models.Db {

    public class ExpEntryOptions {

        public List<int> ExperienceIDs { get; set; } = new List<int>();

        public short FactionID { get; set; }

        public int Interval { get; set; }

        public short WorldID { get; set; }

    }
}
