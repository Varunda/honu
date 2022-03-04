using System.Collections.Generic;
using watchtower.Models.Census;
using watchtower.Models.Db;

namespace watchtower.Models.Alert {

    public class ExpandedAlertParticipants {

        public List<AlertParticipantDataEntry> Entries { get; set; } = new List<AlertParticipantDataEntry>();

        public List<PsCharacter> Characters { get; set; } = new List<PsCharacter>();

        public List<PsOutfit> Outfits { get; set; } = new List<PsOutfit>();

        public List<Session> Sessions { get; set; } = new List<Session>();

    }
}
