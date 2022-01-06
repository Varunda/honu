using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models.Census;
using watchtower.Models.Db;
using watchtower.Models.Events;

namespace watchtower.Models.Report {

    public class OutfitReport {

        public Guid ID { get; set; } = Guid.Empty;

        public string Generator { get; set; } = "";

        /// <summary>
        ///     -1 means not set yet
        /// </summary>
        public short TeamID { get; set; } = -1;

        /// <summary>
        ///     Optional zone filter
        /// </summary>
        public uint? ZoneID { get; set; } = null;

        public List<string> Players { get; set; } = new List<string>();

        public DateTime Timestamp { get; set; }

        public DateTime PeriodStart { get; set; }

        public DateTime PeriodEnd { get; set; }

        public List<string> CharacterIDs { get; set; } = new List<string>();

        public List<KillEvent> Kills { get; set; } = new List<KillEvent>();

        public List<KillEvent> Deaths { get; set; } = new List<KillEvent>();

        public List<ExpEvent> Experience { get; set; } = new List<ExpEvent>();

        public List<FacilityControlEvent> Control { get; set; } = new List<FacilityControlEvent>();

        public List<PlayerControlEvent> PlayerControl { get; set; } = new List<PlayerControlEvent>();

        public List<PsFacility> Facilities { get; set; } = new List<PsFacility>();

        public List<PsItem> Items { get; set; } = new List<PsItem>();

        public List<PsCharacter> Characters { get; set; } = new List<PsCharacter>();

        public List<PsOutfit> Outfits { get; set; } = new List<PsOutfit>();

        public List<Session> Sessions { get; set; } = new List<Session>();

    }


}
