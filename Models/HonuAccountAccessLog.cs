using System;

namespace watchtower.Models {

    public class HonuAccountAccessLog {

        public long ID { get; set; }

        public DateTime Timestamp { get; set; }

        public bool Success { get; set; }

        public long? HonuID { get; set; }

        public string? Email { get; set; }

    }
}
