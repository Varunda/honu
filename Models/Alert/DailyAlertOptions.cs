using System.Collections.Generic;

namespace watchtower.Models.Alert {

    public class DailyAlertOptions {

        public List<DailyAlertConfigEntry> Worlds { get; set; } = new();

    }
}
