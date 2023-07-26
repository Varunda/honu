using System;
using System.Collections.Generic;

namespace watchtower.Models.Db {

    public class RealtimeMapState {

        public long ID { get; set; }

        public short WorldID { get; set; }

        public uint ZoneID { get; set; }

        /// <summary>
        ///     This is when this data was last updated from litha's API
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        ///     This is when this data was saved in the DB. Will be the <c>default</c> <see cref="DateTime"/> if not saved
        /// </summary>
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

        /// <summary>
        ///     Check if there is at least one faction that has at least one player
        /// </summary>
        /// <returns></returns>
        public bool HasOneFaction() {
            return this.FactionBounds.VS > 0 || this.FactionBounds.NC > 0 || this.FactionBounds.TR > 0;
        }

        /// <summary>
        ///     Check if there are two factions that have at least one player
        /// </summary>
        /// <returns></returns>
        public bool HasTwoFactions() {
            return (this.FactionBounds.VS > 0 && this.FactionBounds.NC > 0)
                || (this.FactionBounds.VS > 0 && this.FactionBounds.TR > 0)
                || (this.FactionBounds.NC > 0 && this.FactionBounds.TR > 0);
        }

        public int GetLowerBounds() {
            return GetLowerBounds(this.FactionBounds.VS) + GetLowerBounds(this.FactionBounds.NC) + GetLowerBounds(this.FactionBounds.TR);
        }

        /// <summary>
        ///     Get the upper bounds of the number of players who are in this region
        /// </summary>
        /// <returns></returns>
        public int GetUpperBounds() {
            return this.FactionBounds.VS + this.FactionBounds.NC + this.FactionBounds.TR;
        }

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

        public static bool operator ==(RealtimeMapState? a, RealtimeMapState? b) {
            if (ReferenceEquals(a, b)) {
                return true;
            } else if (a is null) {
                return false;
            } else if (b is null) {
                return false;
            }

            return a.Equals(b);
        }

        public static bool operator !=(RealtimeMapState? a, RealtimeMapState? b) {
            return !(a == b);
        }

        public override int GetHashCode() {
            return HashCode.Combine(WorldID, ZoneID, RegionID);
        }

        public string GetDifference(RealtimeMapState other) {
            string s = "";

            if (this.CaptureTimeLeftMs != other.CaptureTimeLeftMs) {
                s += $"[CaptureTimeLeftMs: {CaptureTimeLeftMs - other.CaptureTimeLeftMs}] ";
            }
            if (this.FactionBounds != other.FactionBounds) {
                s += $"[FactionBounds: {FactionBounds.VS - other.FactionBounds.VS} {FactionBounds.NC - other.FactionBounds.NC} {FactionBounds.TR - other.FactionBounds.TR}] ";
            }
            if (this.FactionPercentage != other.FactionPercentage) {
                s += $"[FactionPercentage: {FactionPercentage.VS - other.FactionPercentage.VS} {FactionPercentage.NC - other.FactionPercentage.NC} {FactionPercentage.TR - other.FactionPercentage.TR}] ";
            }

            return s;
        }

        private static int GetLowerBounds(int maxBound) {
            if (maxBound == 0) {
                return 0;
            }

            if (maxBound == 12) {
                return 1;
            }

            if (maxBound == 24) {
                return 12;
            }

            if (maxBound == 48) {
                return 24;
            }

            if (maxBound == 96) {
                return 48;
            }

            // 96+
            if (maxBound == 192) {
                return 96;
            }

            return maxBound - 1;
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

        public override int GetHashCode() {
            return HashCode.Combine(VS, NC, TR, NS);
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

        public override int GetHashCode() {
            return HashCode.Combine(VS, NC, TR, NS);
        }
    }


}
