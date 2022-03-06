using watchtower.Models.Census;

namespace watchtower.Models.Alert {

    /// <summary>
    ///     Represents data about the different classes a player may have played
    /// </summary>
    public class AlertPlayerProfileData {

        /// <summary>
        ///     Unique ID of the entry
        /// </summary>
        public long ID { get; set; }

        /// <summary>
        ///     ID of the <see cref="PsAlert"/> this profile data is for
        /// </summary>
        public long AlertID { get; set; }

        /// <summary>
        ///     ID of the character this profile data is for
        /// </summary>
        public string CharacterID { get; set; } = "";

        /// <summary>
        ///     Profile ID
        /// </summary>
        public short ProfileID { get; set; }

        /// <summary>
        ///     How many kills this character got with the given profile ID
        /// </summary>
        public int Kills { get; set; }

        /// <summary>
        ///     How many deaths this character had with the given profile ID
        /// </summary>
        public int Deaths { get; set; }

        /// <summary>
        ///     How many vehicle kills this character had with the given profile ID
        /// </summary>
        public int VehicleKills { get; set; }

        /// <summary>
        ///     How many seconds a character was this profile 
        /// </summary>
        public int TimeAs { get; set; }

    }
}
