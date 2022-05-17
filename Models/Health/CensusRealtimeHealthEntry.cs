using System;

namespace watchtower.Models {

    public class CensusRealtimeHealthEntry {

        public DateTime? LastEvent { get; set; }

        public int FailureCount { get; set; }

    }
}
