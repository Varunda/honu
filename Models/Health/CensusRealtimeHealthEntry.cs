using System;

namespace watchtower.Models {

    public class CensusRealtimeHealthEntry {

        public short WorldID { get; set; }

        public DateTime? LastEvent { get; set; }

        public int FailureCount { get; set; }

    }
}
