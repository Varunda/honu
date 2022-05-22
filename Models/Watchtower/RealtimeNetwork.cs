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

        public string Display { get; set; } = "";

        public short FactionID { get; set; }

        public List<RealtimeNetworkInteraction> Interactions { get; set; } = new();

    }

    public class RealtimeNetworkInteraction {

        public string OtherID { get; set; } = "";

        public string OtherName { get; set; } = "";

        public short FactionID { get; set; }

        public double Strength { get; set; }

    }

}
