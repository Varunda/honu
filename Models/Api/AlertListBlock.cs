using System.Collections.Generic;
using watchtower.Models.Census;

namespace watchtower.Models.Api {

    public class AlertListBlock {

        public List<PsAlert> Alerts { get; set; } = [];

        public List<PsMetagameEvent> MetagameEvents { get; set; } = [];

    }
}
