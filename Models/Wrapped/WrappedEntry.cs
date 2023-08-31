using System;
using System.Collections.Generic;
using watchtower.Code.Constants;
using watchtower.Code.ExtensionMethods;
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
        ///     When this wrapped entry was created. Needed for viewing past data
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        ///     Character IDs the data is generated from
        /// </summary>
        public List<string> InputCharacterIDs { get; set; } = new();

        /// <summary>
        ///     What status this wrapped entry is in. See <see cref="WrappedEntryStatus"/>
        /// </summary>
        public int Status { get; set; } = WrappedEntryStatus.UNKNOWN;

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
        ///     All <see cref="FireGroupToFireMode"/> that might be referenced while displaying the wrapped data
        /// </summary>
        public Dictionary<int, List<FireGroupToFireMode>> FireGroupToFireModes { get; set; } = new();

        /// <summary>
        ///     All vehicles
        /// </summary>
        public Dictionary<int, PsVehicle> Vehicles { get; set; } = new();

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
        ///     All the vehicle destroy events the input characters had
        /// </summary>
        public List<VehicleDestroyEvent> VehicleDestroy { get; set; } = new();

        /// <summary>
        ///     All the control events the input characters participated in
        /// </summary>
        public List<FacilityControlEvent> ControlEvents { get; set; } = new();

        /// <summary>
        ///     All the achievement earned events the input characters earned
        /// </summary>
        public List<AchievementEarnedEvent> AchievementEarned { get; set; } = new();

        /// <summary>
        ///     All the item added events the input characters earned
        /// </summary>
        public List<ItemAddedEvent> ItemAddedEvents { get; set; } = new();

    }

    public class WrappedEntryIdSet {

        public HashSet<string> Characters { get; set; } = new();

        public HashSet<string> Outfits { get; set; } = new();

        public HashSet<int> Items { get; set; } = new();

        public HashSet<int> ExperienceTypes { get; set; } = new();

        public HashSet<int> Achievements { get; set; } = new();

        public HashSet<int> Facilities { get; set; } = new();

        public HashSet<string> Vehicles { get; set; } = new();

        public HashSet<int> FireGroupXrefs { get; set; } = new();

        public static WrappedEntryIdSet FromEntry(WrappedEntry entry) {
            WrappedEntryIdSet set = new();
            set.AddFromEntry(entry);

            return set;
        }

        public void AddFromEntry(WrappedEntry entry) {

            foreach (string charID in entry.InputCharacterIDs) {
                Characters.Add(charID);
            }

            foreach (KillEvent ev in entry.Kills) {
                Characters.Add(ev.AttackerCharacterID);
                Characters.Add(ev.KilledCharacterID);

                Items.Add(ev.WeaponID);
                FireGroupXrefs.Add(ev.AttackerFireModeID);

                if (ev.AttackerVehicleID != 0) {
                    Vehicles.Add(ev.AttackerVehicleID.ToString());
                }
            }

            foreach (KillEvent ev in entry.Deaths) {
                Characters.Add(ev.AttackerCharacterID);
                Characters.Add(ev.KilledCharacterID);

                if (ev.AttackerVehicleID != 0) {
                    Vehicles.Add(ev.AttackerVehicleID.ToString());
                }

                Items.Add(ev.WeaponID);
                FireGroupXrefs.Add(ev.AttackerFireModeID);
            }

            foreach (ExpEvent ev in entry.Experience) {
                Characters.Add(ev.SourceID);
                if (ev.OtherID.Length == 19) {
                    Characters.Add(ev.OtherID);
                }

                ExperienceTypes.Add(ev.ExperienceID);
            }

            foreach (FacilityControlEvent ev in entry.ControlEvents) {
                Facilities.Add(ev.FacilityID);
            }

            foreach (VehicleDestroyEvent ev in entry.VehicleDestroy) {
                Characters.Add(ev.AttackerCharacterID);
                Characters.Add(ev.KilledCharacterID);

                Items.Add(ev.AttackerWeaponID);

                Vehicles.Add(ev.AttackerVehicleID);
                Vehicles.Add(ev.KilledVehicleID);
            }

            foreach (AchievementEarnedEvent ev in entry.AchievementEarned) {
                Achievements.Add(ev.AchievementID);
            }

            foreach (ItemAddedEvent ev in entry.ItemAddedEvents) {
                Items.Add(ev.ItemID);
            }

            foreach (AchievementEarnedEvent ev in entry.AchievementEarned) {
                Achievements.Add(ev.AchievementID);
            }

        }

    }

    public class WrappedEntryApiInput {

        public List<string> IDs { get; set; } = new();

    }

    /// <summary>
    ///     Saved JSON file for a character to save DB lookups
    /// </summary>
    public class WrappedSavedCharacterData {

        /// <summary>
        ///     ID of the character this is for
        /// </summary>
        public string CharacterID { get; set; } = "";

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
        ///     All the vehicle destroy events the input characters had
        /// </summary>
        public List<VehicleDestroyEvent> VehicleDestroy { get; set; } = new();

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
        public List<ItemAddedEvent> ItemAddedEvents { get; set; } = new();


    }

}
