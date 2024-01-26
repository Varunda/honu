using System.Collections.Generic;
using watchtower.Models.Alert;
using watchtower.Models.Census;

namespace watchtower.Models.Api {

    public class ExpandedCharacterAlerts {

        public string CharacterID { get; set; } = "";

        public List<CharacterAlertPlayer> Alerts { get; set; } = new();

        public List<PsMetagameEvent> MetagameEvents { get; set; } = new();

    }
}
