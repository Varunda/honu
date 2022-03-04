namespace watchtower.Models.Alert {

    /// <summary>
    ///     Represents data about someone who participated in an alert
    /// </summary>
    public class AlertParticipant {

        /// <summary>
        ///     ID of the character
        /// </summary>
        public string CharacterID { get; set; } = "";

        /// <summary>
        ///     How many seconds the character was online
        /// </summary>
        public int SecondsOnline { get; set; }

    }
}
