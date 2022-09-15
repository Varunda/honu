using System;
using System.Collections.Generic;

namespace watchtower.Models.Watchtower {

    public class RealtimeNetwork {

        public short? WorldID { get; set; }

        public uint? ZoneID { get; set; }

        public DateTime Timestamp { get; set; }

        public List<RealtimeNetworkPlayer> Players { get; set; } = new List<RealtimeNetworkPlayer>();

    }

    public class RealtimeNetworkPlayer {

        public string CharacterID { get; set; } = "";

        public string? OutfitID { get; set; } = null;

        public short FactionID { get; set; }

        public short TeamID { get; set; }

        public List<RealtimeNetworkInteraction> Interactions { get; set; } = new();

    }

    public class RealtimeNetworkInteraction {

        public string OtherID { get; set; } = "";

        public string? OutfitID { get; set; } = null;

        public short FactionID { get; set; }

        public short TeamID { get; set; }

        public double Strength { get; set; }

    }

}
