using System;

namespace watchtower.Models.Events {

    public class FishCaughtEvent {

        public ulong ID { get; set; }

        public string CharacterID { get; set; } = "";

        public int FishID { get; set; }

        public DateTime Timestamp { get; set; }

        public uint ZoneID { get; set; }

        public short WorldID { get; set; }

        public short TeamID { get; set; }

        public short LoadoutID { get; set; }

    }
}
