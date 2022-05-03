using System;

namespace watchtower.Models.Alert {

    public class AlertPopulation {

        /// <summary>
        ///     Unique ID of the entry
        /// </summary>
        public long ID { get; set; }

        /// <summary>
        ///     ID of the alert this entry is for
        /// </summary>
        public long AlertID { get; set; }

        /// <summary>
        ///     The timestamp of when this sample was taken
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        ///     How many characters were on VS at this time
        /// </summary>
        public int CountVS { get; set; }

        /// <summary>
        ///     How many characters were on NC at this time
        /// </summary>
        public int CountNC { get; set; }

        /// <summary>
        ///     How many characters were on TR at this time
        /// </summary>
        public int CountTR { get; set; }

        /// <summary>
        ///     How many characters were on an unknown faction at this time, such as NS that doesn't have a set team_id
        /// </summary>
        public int CountUnknown { get; set; }

    }
}
