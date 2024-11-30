using System;

namespace watchtower.Models.Db {

    public class WorldChange {

        public string CharacterID { get; set; } = "";

        public short WorldID { get; set; }

        public DateTime Timestamp { get; set; }

    }
}
