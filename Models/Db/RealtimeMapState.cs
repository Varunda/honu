using System;

namespace watchtower.Models.Db {

    public class RealtimeMapState {

        public long ID { get; set; }

        public short WorldID { get; set; }

        public uint ZoneID { get; set; }

        public DateTime Timestamp { get; set; }

        public DateTime SaveTimestamp { get; set; }

        public int RegionID { get; set; }

        public int OwningFactionID { get; set; }

        public bool Contested { get; set; }

        public int ContestingFactionID { get; set; }

        public int CaptureTimeMs { get; set; }

        public int CaptureTimeLeftMs { get; set; }

        public int CaptureFlagsCount { get; set; }

        public int CaptureFlagsLeft { get; set; }

        public RealtimeMapStateFactionBounds FactionBounds { get; set; } = new();

        public RealtimeMapStateFactionPopulationPercentage FactionPercentage { get; set; } = new();

    }

    public class RealtimeMapStateFactionBounds {

        public int VS { get; set; }

        public int NC { get; set; }

        public int TR { get; set; }

        public int NS { get; set; }

    }

    public class RealtimeMapStateFactionPopulationPercentage {

        public decimal VS { get; set; }

        public decimal NC { get; set; }

        public decimal TR { get; set; }

        public decimal NS { get; set; }
        
    }


}
