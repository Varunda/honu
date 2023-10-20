using System;
using System.Collections.Generic;

namespace watchtower.Models.PSB {

    public class PsbOvOAccountSheet {

        public string FileID { get; set; } = "";

        public List<string> Emails { get; set; } = new();

        public DateTime When { get; set; }

        public string State { get; set; } = "";

        public string Type { get; set; } = "";

        public List<PsbOvOAccountSheetUsage> Accounts { get; set; } = new();

    }

    public class PsbOvOAccountSheetUsage {

        public string AccountNumber { get; set; } = "";

        public string Username { get; set; } = "";

        public string? Player { get; set; } = null;

    }

}
