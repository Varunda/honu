using System;

namespace watchtower.Models.Events {

    public class ItemAddedEvent {

        public long ID { get; set; }

        public string CharacterID { get; set; } = "";

        public int ItemID { get; set; }

        public int ItemCount { get; set; }

        public string Context { get; set; } = "";

        public DateTime Timestamp { get; set; }

        public uint ZoneID { get; set; }

        public short WorldID { get; set; }

    }
}
