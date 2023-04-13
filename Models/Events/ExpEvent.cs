using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Models.Events {

    public class ExpEvent {

        public ulong ID { get; set; }

        public string SourceID { get; set; } = "";

        public int ExperienceID { get; set; }

        public short LoadoutID { get; set; }

        public short TeamID { get; set; }

        public string OtherID { get; set; } = "";

        public int Amount { get; set; }

        public short WorldID { get; set; }

        public uint ZoneID { get; set; }

        public DateTime Timestamp { get; set; }

    }
}
