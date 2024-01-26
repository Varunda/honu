using System;
using watchtower.Models.Census;

namespace watchtower.Models.Alert {

    /// <summary>
    ///     Represents data about someone who participated in an alert
    /// </summary>
    public class CharacterAlertPlayer {

        // info about the alert

        /// <summary>
        ///     ID of the <see cref="PsAlert"/> this character played in
        /// </summary>
        public long AlertID { get; set; }

        public short? VictorFactionID { get; set; }

        public int Duration { get; set; }

        /// <summary>
        ///     What zone the alert took place in
        /// </summary>
        public uint ZoneID { get; set; }

        /// <summary>
        ///     World the alert took place in
        /// </summary>
        public short WorldID { get; set; }

        /// <summary>
        ///     What metagame event triggered this
        /// </summary>
        public int MetagameAlertID { get; set; }

        /// <summary>
        ///     Incremental ID 
        /// </summary>
        public int InstanceID { get; set; }

        /// <summary>
        ///     Name of the alert
        /// </summary>
        public string Name { get; set; } = "";

        // info about the character

        public string CharacterID { get; set; } = "";

        public string? OutfitID { get; set; } = null;

        public int SecondsOnline { get; set; }

        public DateTime Timestamp { get; set; }

        // Kills

        public int Kills { get; set; }

        public int Deaths { get; set; }

        public int VehicleKills { get; set; }

        // Support

        public int Heals { get; set; }

        public int Revives { get; set; }

        public int ShieldRepairs { get; set; }

        public int Resupplies { get; set; }

        public int Spawns { get; set; }

        public int Repairs { get; set; }

    }
}
