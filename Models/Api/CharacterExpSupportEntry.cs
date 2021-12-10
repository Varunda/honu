
namespace watchtower.Models.Api {

    /// <summary>
    ///     Represents the character a support event was performed on. A support event is an exp event that another character
    ///     recieved the benefit of, such as a heal or revive
    /// </summary>
    public class CharacterExpSupportEntry {

        /// <summary>
        ///     ID of the character that received the support
        /// </summary>
        public string CharacterID { get; set; } = "";

        /// <summary>
        ///     Name of the character that received the support
        /// </summary>
        public string CharacterName { get; set; } = "";

        /// <summary>
        ///     How many times this character received support
        /// </summary>
        public int Amount { get; set; }

    }

}