using System.Collections.Generic;
using watchtower.Models.Census;
using watchtower.Models.Db;

namespace watchtower.Models.Alert {

    public class ExpandedAlertPlayers {

        public List<AlertPlayerDataEntry> Entries { get; set; } = new List<AlertPlayerDataEntry>();

        public List<AlertPlayerProfileData> ProfileData { get; set; } = new List<AlertPlayerProfileData>();

        public List<MinimalCharacter> Characters { get; set; } = new List<MinimalCharacter>();

        public List<PsOutfit> Outfits { get; set; } = new List<PsOutfit>();

    }
}
