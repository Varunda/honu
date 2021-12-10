using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Models.Census {

    /// <summary>
    ///     Represents information about a Planetside 2 character
    /// </summary>
    public class PsCharacter {

        /// <summary>
        ///     ID of the character
        /// </summary>
        public string ID { get; set; } = "";

        /// <summary>
        ///     Name of the character
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        ///     ID of the world the character is one
        /// </summary>
        public short WorldID { get; set; }

        /// <summary>
        ///     ID of the outfit, if the character is in one
        /// </summary>
        public string? OutfitID { get; set; }

        /// <summary>
        ///     Tag of the outfit, if the character is in one
        /// </summary>
        public string? OutfitTag { get; set; }

        /// <summary>
        ///     Name of the outfit, if the character is in one
        /// </summary>
        public string? OutfitName { get; set; }

        /// <summary>
        ///     Faction ID of the character
        /// </summary>
        public short FactionID { get; set; }

        /// <summary>
        ///     Battle rank of the character
        /// </summary>
        public short BattleRank { get; set; }

        /// <summary>
        ///     How many times the character has ASPed
        /// </summary>
        public int Prestige { get; set; }

        /// <summary>
        ///     <c>DateTime</c> of when the last update on this character occured
        /// </summary>
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        /// <summary>
        ///     When the character was created
        /// </summary>
        public DateTime DateCreated { get; set; }

        /// <summary>
        ///     When the character last logged in
        /// </summary>
        public DateTime DateLastLogin { get; set; }

        /// <summary>
        ///     When the charactere was last saved into Census
        /// </summary>
        public DateTime DateLastSave { get; set; }

    }

    /// <summary>
    ///     Extension methods for <see cref="PsCharacter"/>s
    /// </summary>
    public static class PsCharacterExtensionMethods {

        /// <summary>
        ///     Get the display of a character, which includes the [TAG] of the outfit they're in,
        ///     if they are in one
        /// </summary>
        /// <param name="c">Extension instance</param>
        public static string GetDisplayName(this PsCharacter c) {
            return $"{(c.OutfitID != null ? $"[{c.OutfitTag}] " : "")}{c.Name}";
        }

    }

}
