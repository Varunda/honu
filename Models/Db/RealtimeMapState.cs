using System;
using System.Collections.Generic;

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

        public override bool Equals(object? obj) {
            return obj is RealtimeMapState state &&
                   // ID == state.ID && // ID is not included either
                   WorldID == state.WorldID &&
                   ZoneID == state.ZoneID &&
                   // Timestamp == state.Timestamp && // do not include timestamp. It is possible the data is updated, but the data itself has not changed
                   // SaveTimestamp == state.SaveTimestamp &&
                   RegionID == state.RegionID &&
                   OwningFactionID == state.OwningFactionID &&
                   Contested == state.Contested &&
                   ContestingFactionID == state.ContestingFactionID &&
                   CaptureTimeMs == state.CaptureTimeMs &&
                   CaptureTimeLeftMs == state.CaptureTimeLeftMs &&
                   CaptureFlagsCount == state.CaptureFlagsCount &&
                   CaptureFlagsLeft == state.CaptureFlagsLeft &&
                   EqualityComparer<RealtimeMapStateFactionBounds>.Default.Equals(FactionBounds, state.FactionBounds) &&
                   EqualityComparer<RealtimeMapStateFactionPopulationPercentage>.Default.Equals(FactionPercentage, state.FactionPercentage);
        }

        public static bool operator ==(RealtimeMapState a, RealtimeMapState b) {
            if (ReferenceEquals(a, b)) {
                return true;
            } else if (a is null) {
                return false;
            } else if (b is null) {
                return false;
            }

            return a.Equals(b);
        }

        public static bool operator !=(RealtimeMapState a, RealtimeMapState b) {
            return !(a == b);
        }

    }

    public class RealtimeMapStateFactionBounds {

        public int VS { get; set; }

        public int NC { get; set; }

        public int TR { get; set; }

        public int NS { get; set; }

        public override bool Equals(object? obj) {
            return obj is RealtimeMapStateFactionBounds bounds &&
                   VS == bounds.VS &&
                   NC == bounds.NC &&
                   TR == bounds.TR &&
                   NS == bounds.NS;
        }
    }

    public class RealtimeMapStateFactionPopulationPercentage {

        public decimal VS { get; set; }

        public decimal NC { get; set; }

        public decimal TR { get; set; }

        public decimal NS { get; set; }

        public override bool Equals(object? obj) {
            return obj is RealtimeMapStateFactionPopulationPercentage percentage &&
                   VS == percentage.VS &&
                   NC == percentage.NC &&
                   TR == percentage.TR &&
                   NS == percentage.NS;
        }
    }


}
