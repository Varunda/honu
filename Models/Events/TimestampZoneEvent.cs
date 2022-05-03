using System;

namespace watchtower.Models.Events {

    public class TimestampZoneEvent {

        public DateTime Timestamp;

        public uint ZoneID;

        public short LoadoutID;

        public string Type = "";

        public short TeamID = 0;

    }
}
