using System;

namespace watchtower.Models.Census {

    public class PsAlert {

        /// <summary>
        ///     Unique ID of the alert
        /// </summary>
        public long ID { get; set; }

        /// <summary>
        ///     When this alert was triggered
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        ///     How long, in seconds, the alert will last for
        /// </summary>
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
        public int AlertID { get; set; }

        /// <summary>
        ///     Who won the alert, null if alert hasn't concluded
        /// </summary>
        public short? VictorFactionID { get; set; }

        /// <summary>
        ///     ID of the warpgate VS owns at the start of the alert
        /// </summary>
        public int WarpgateVS { get; set; }

        /// <summary>
        ///     ID of the warpgate NC owns at the start of the alert
        /// </summary>
        public int WarpgateNC { get; set; }

        /// <summary>
        ///     ID of the warpgate TR owns at the start of the alert
        /// </summary>
        public int WarpgateTR { get; set; }

        public int ZoneFacilityCount { get; set; }

        public int? CountVS { get; set; }

        public int? CountNC { get; set; }

        public int? CountTR { get; set; }

    }
}
