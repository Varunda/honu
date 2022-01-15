using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models.Census;
using watchtower.Models.Db;
using watchtower.Models.Events;

namespace watchtower.Models.Report {

    public class OutfitReport {

        /// <summary>
        ///     Unique ID of the report. Used to easily share reports
        /// </summary>
        public Guid ID { get; set; } = Guid.Empty;

        /// <summary>
        ///     Generator used to make the report. This is a DSL I guess. I wish I kept it as JSON instead lol
        /// </summary>
        public string Generator { get; set; } = "";

        /// <summary>
        ///     -1 means not set yet
        /// </summary>
        public short TeamID { get; set; } = -1;

        /// <summary>
        ///     Optional zone filter
        /// </summary>
        public uint? ZoneID { get; set; } = null;

        /// <summary>
        ///     List of the player IDs that were tracked
        /// </summary>
        public List<string> Players { get; set; } = new List<string>();

        /// <summary>
        ///     List of outfits that were tracked
        /// </summary>
        public List<PsOutfit> TrackedOutfits { get; set; } = new List<PsOutfit>();

        /// <summary>
        ///     List of characters that are tracked
        /// </summary>
        public List<PsCharacter> TrackedCharacters { get; set; } = new List<PsCharacter>();

        public DateTime Timestamp { get; set; }

        public DateTime PeriodStart { get; set; }

        public DateTime PeriodEnd { get; set; }

        public List<string> CharacterIDs { get; set; } = new List<string>();

        public List<KillEvent> Kills { get; set; } = new List<KillEvent>();

        public List<KillEvent> Deaths { get; set; } = new List<KillEvent>();

        public List<ExpEvent> Experience { get; set; } = new List<ExpEvent>();

        public List<FacilityControlEvent> Control { get; set; } = new List<FacilityControlEvent>();

        /// <summary>
        ///     All player control events relevant to the players tracked
        /// </summary>
        public List<PlayerControlEvent> PlayerControl { get; set; } = new List<PlayerControlEvent>();

        /// <summary>
        ///     Lise of all facilities that may show up in a report, used to save API calls
        /// </summary>
        public List<PsFacility> Facilities { get; set; } = new List<PsFacility>();

        /// <summary>
        ///     All items that may show up in a report, used to save API calls
        /// </summary>
        public List<PsItem> Items { get; set; } = new List<PsItem>();

        /// <summary>
        ///     All characters that may show up in the report, used to save API calls
        /// </summary>
        public List<PsCharacter> Characters { get; set; } = new List<PsCharacter>();

        /// <summary>
        ///     All outfits that show up in the report, used to save API calls
        /// </summary>
        public List<PsOutfit> Outfits { get; set; } = new List<PsOutfit>();

        /// <summary>
        ///     All sessions that the tracked players had during the time frame
        /// </summary>
        public List<Session> Sessions { get; set; } = new List<Session>();

    }


}
