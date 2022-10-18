using System;

namespace watchtower.Models.Db {

    public class WeaponStatTop {

        public long ID { get; set; }

        public short WorldID { get; set; }

        public short FactionID { get; set; }

        public int ItemID { get; set; }

        public short TypeID { get; set; }

        public DateTime Timestamp { get; set; }

        public long ReferenceID { get; set; }

    }
}
