using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models.Census;
using watchtower.Models.Db;
using watchtower.Models.Events;
using watchtower.Models.Health;

namespace watchtower.Models.Report {

    public class OutfitReport {

        /// <summary>
        ///     Parameters used to generate this report
        /// </summary>
        public OutfitReportParameters Parameters { get; set; } = new();

        /// <summary>
        ///     This when report was generated
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        ///     List of outfits that were tracked
        /// </summary>
        public List<PsOutfit> TrackedOutfits { get; set; } = new List<PsOutfit>();

        /// <summary>
        ///     List of characters that are tracked
        /// </summary>
        public List<PsCharacter> TrackedCharacters { get; set; } = new List<PsCharacter>();

        /// <summary>
        ///     List of kills that tracked characters got during the period of the report
        /// </summary>
        public List<KillEvent> Kills { get; set; } = new List<KillEvent>();

        /// <summary>
        ///     Deaths that tracked characters got during the period of the report
        /// </summary>
        public List<KillEvent> Deaths { get; set; } = new List<KillEvent>();

        /// <summary>
        ///     Exp events that tracked characters got during the period of the report
        /// </summary>
        public List<ExpEvent> Experience { get; set; } = new List<ExpEvent>();

        /// <summary>
        ///     Vehicle destroy events that tracked characters got during the period of the report
        /// </summary>
        public List<VehicleDestroyEvent> VehicleDestroy { get; set; } = new();

        /// <summary>
        ///     Capture/Defend events that tracked outfits got during the alert
        /// </summary>
        public List<FacilityControlEvent> Control { get; set; } = new List<FacilityControlEvent>();

        /// <summary>
        ///     Has all player control events for the tracker characters, and any control events
        ///     the tracked character participated in
        /// </summary>
        public List<PlayerControlEvent> PlayerControl { get; set; } = new List<PlayerControlEvent>();

        /// <summary>
        ///     List of achievements earned by tracked characters
        /// </summary>
        public List<AchievementEarnedEvent> AchievementsEarned { get; set; } = new();

        public List<Achievement> Achievements { get; set; } = new();

        /// <summary>
        ///     Lise of all facilities that may show up in a report, used to save API calls
        /// </summary>
        public List<PsFacility> Facilities { get; set; } = new List<PsFacility>();

        /// <summary>
        ///     All items that may show up in a report, used to save API calls
        /// </summary>
        public List<PsItem> Items { get; set; } = new List<PsItem>();

        /// <summary>
        ///     All item categories
        /// </summary>
        public List<ItemCategory> ItemCategories { get; set; } = new();

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

        /// <summary>
        ///     All reconnects that happened on the server this report takes place on during the time frame
        /// </summary>
        public List<RealtimeReconnectEntry> Reconnects { get; set; } = new();

    }


}
