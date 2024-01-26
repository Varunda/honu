namespace watchtower.Models.Alert {

    /// <summary>
    ///     represents who played in an alert, but not the data about each character. see <see cref="CharacterAlertPlayer"/> for that
    /// </summary>
    public class AlertPlayer {

        /// <summary>
        ///     ID of the character
        /// </summary>
        public string CharacterID { get; set; } = "";

    }
}
