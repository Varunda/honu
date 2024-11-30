using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Models.Db {

    public class KillDbOptions {

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public int Interval { get; set; }

        public short FactionID { get; set; }

        public short WorldID { get; set; }

    }
}
