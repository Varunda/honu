using System;
using watchtower.Models.Census;

namespace watchtower.Models.Db {

    /// <summary>
    ///     contains information about a name change of a character
    /// </summary>
    public class CharacterNameChange {

        /// <summary>
        ///     unique ID
        /// </summary>
        public long ID { get; set; }

        /// <summary>
        ///     ID of <see cref="PsCharacter"/> this change is for
        /// </summary>
        public string CharacterID { get; set; } = "";

        /// <summary>
        ///     old name of the character
        /// </summary>
        public string OldName { get; set; } = "";

        /// <summary>
        ///     new name of the character
        /// </summary>
        public string NewName { get; set; } = "";

        /// <summary>
        ///     the earliest this name change could have taken place. usually, this is the last time honu updated a character
        /// </summary>
        public DateTime LowerBound { get; set; }

        /// <summary>
        ///     the latest the name change could have taken place. usually, this is when the character was last updated in the census API
        /// </summary>
        public DateTime UpperBound { get; set; }

        /// <summary>
        ///     timestamp of when this entry was created
        /// </summary>
        public DateTime Timestamp { get; set; }

    }
}
