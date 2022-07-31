using System;

namespace watchtower.Models.Events {

    public class AchievementEarnedEvent {

        public long ID { get; set; }

        public string CharacterID { get; set; } = "";

        public DateTime Timestamp { get; set; }

        public int AchievementID { get; set; }

        public uint ZoneID { get; set; }

        public short WorldID { get; set; }

    }
}
