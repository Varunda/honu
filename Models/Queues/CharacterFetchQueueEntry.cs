using watchtower.Code.Constants;

namespace watchtower.Models.Queues {

    public class CharacterFetchQueueEntry {

        /// <summary>
        ///     ID of the character to fetch
        /// </summary>
        public string CharacterID { get; set; } = "";

        /// <summary>
        ///     Once cached, will the character be loaded into the <see cref="CharacterStore"/>? Defaults to <c>true</c>.
        ///     If <c>false</c>, it will not be put. Useful for when lots of characters are loaded at once,
        ///     but do not need to be kept in memory once loaded (such as loading an outfit, where most of the characters
        ///     will not be online, and do not need to be looked up further)
        /// </summary>
        public bool Store { get; set; } = true;

        /// <summary>
        ///     What environment the character will exist on
        /// </summary>
        public CensusEnvironment Environment { get; set; } = CensusEnvironment.UNKNOWN;

    }
}
