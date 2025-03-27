using System;

namespace watchtower.Models.Api {

    public class NameFightEntry {

        public DateTime Timestamp { get; set; }

        public short WorldID { get; set; }

        public int Total { get; set; }

        public int WinsVs { get; set; }

        public int WinsNc { get; set; }

        public int WinsTr { get; set; }

    }
}
