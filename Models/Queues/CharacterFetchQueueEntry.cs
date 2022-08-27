using watchtower.Code.Constants;

namespace watchtower.Models.Queues {

    public class CharacterFetchQueueEntry {

        public string CharacterID { get; set; } = "";

        public bool Store { get; set; } = true;

        /// <summary>
        ///     What environment the character will exist on
        /// </summary>
        public CensusEnvironment Environment { get; set; } = CensusEnvironment.UNKNOWN;

    }
}
