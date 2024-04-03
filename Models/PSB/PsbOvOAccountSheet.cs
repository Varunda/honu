using System;
using System.Collections.Generic;
using watchtower.Models.Db;

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

    public class PsbOvOAccountUsage {

        public PsbOvOAccountSheetUsage SheetUsage { get; set; }

        public List<Session> Sessions { get; set; } = new();

        public PsbOvOAccountUsage(PsbOvOAccountSheetUsage sheetUsage) {
            SheetUsage = sheetUsage;
        }

    }

}
