using System;
using System.Collections.Generic;
using watchtower.Models.Census;
using watchtower.Models.Db;
using watchtower.Models.Events;

namespace watchtower.Models.Wrapped {

    public class WrappedEntry {

        /// <summary>
        ///     Unique ID of the wrapped data
        /// </summary>
        public Guid ID { get; set; } = Guid.Empty;

        /// <summary>
        ///     Character IDs the data is generated from
        /// </summary>
        public List<string> InputCharacterIDs { get; set; } = new();

        /// <summary>
        ///     All characters that might be referenced while displaying the wrapped data
        /// </summary>
        public Dictionary<string, PsCharacter> Characters { get; set; } = new();

        /// <summary>
        ///     All the outfits that might be referenced while displaying the wrapped data
        /// </summary>
        public Dictionary<string, PsOutfit> Outfits { get; set; } = new();

        /// <summary>
        ///     All the facilities that might be referenced while displaying the wrapped data
        /// </summary>
        public Dictionary<int, PsFacility> Facilities { get; set; } = new();

        /// <summary>
        ///     All the items that might be referenced while displaying the wrapped data
        /// </summary>
        public Dictionary<int, PsItem> Items { get; set; } = new();

        /// <summary>
        ///     All the achievements that might be referenced while displaying the wrapped data
        /// </summary>
        public Dictionary<int, Achievement> Achievements { get; set; } = new();

        /// <summary>
        ///     All experience types that might be referenced while displaying the wrapped data
        /// </summary>
        public Dictionary<int, ExperienceType> ExperienceTypes { get; set; } = new();

        /// <summary>
        ///     All the sessions the input characters had
        /// </summary>
        public List<Session> Sessions { get; set; } = new();

        /// <summary>
        ///     All the kill events the input characters had
        /// </summary>
        public List<KillEvent> Kills { get; set; } = new();

        /// <summary>
        ///     All the death events the input characters had
        /// </summary>
        public List<KillEvent> Deaths { get; set; } = new();

        /// <summary>
        ///     All the experience events the input characters had
        /// </summary>
        public List<ExpEvent> Experience { get; set; } = new();

        /// <summary>
        ///     All the control events the input characters had
        /// </summary>
        public List<FacilityControlEvent> ControlEvents { get; set; } = new();

        /// <summary>
        ///     All the achievement earned events the input characters earned
        /// </summary>
        public List<AchievementEarnedEvent> AchievementEarned { get; set; } = new();

        /// <summary>
        ///     All the item added events the input characters earned
        /// </summary>
        public List<ItemAddedEvent> ItemAddedEvent { get; set; } = new();

    }

    public class WrappedEntryIdSet {

        public HashSet<string> Characters { get; set; } = new();

        public HashSet<string> Outfits { get; set; } = new();

        public HashSet<int> Items { get; set; } = new();

        public HashSet<int> ExperienceTypes { get; set; } = new();

        public HashSet<int> Achievements { get; set; } = new();

        public HashSet<int> Facilities { get; set; } = new();

    }

    public class WrappedEntryApiInput {

        public List<string> IDs { get; set; } = new();

    }

}
