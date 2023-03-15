using System;
using System.Collections.Generic;
using watchtower.Models.Census;

namespace watchtower.Models.Api {

    /// <summary>
    ///     A single entry that represents
    /// </summary>
    public class OutfitHistoryEntry {

        /// <summary>
        ///     ID of the outfit 
        /// </summary>
        public string OutfitID { get; set; } = "";

        /// <summary>
        ///     When the character joined this outfit
        /// </summary>
        public DateTime Start { get; set; }

        /// <summary>
        ///     When the character left this outfit
        /// </summary>
        public DateTime End { get; set; }

        /// <summary>
        ///     How long they were in the outfit. This is not |End| - |Start|, but rather
        ///     the sum of session lengths during this period
        /// </summary>
        public int Duration { get; set; }

    }

    public class OutfitHistoryBlock {

        public string CharacterID { get; set; } = "";

        public List<OutfitHistoryEntry> Entries { get; set; } = new();

        public List<PsOutfit> Outfits { get; set; } = new();

    }

}
