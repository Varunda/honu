using System;

namespace watchtower.Models.Db {

    /// <summary>
    ///     Represents population data at a specific time 
    /// </summary>
    public class PopulationEntry {

        /// <summary>
        ///     Unique ID of the entry
        /// </summary>
        public long ID { get; set; }

        /// <summary>
        ///     When this measurement started, or the lower bounds of when a session had to be started
        ///     to be included
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        ///     How many seconds after <see cref="Timestamp"/> were included
        /// </summary>
        public int Duration { get; set; }

        /// <summary>
        ///     ID of the world this data is for. 0 if all worlds
        /// </summary>
        public short WorldID { get; set; }

        /// <summary>
        ///     ID of the world this data is for. 0 if all factions
        /// </summary>
        public short FactionID { get; set; }

        /// <summary>
        ///     How many sessions took place between <see cref="Timestamp"/> 
        ///     and <see cref="Duration"/> seconds after <see cref="Timestamp"/>
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        ///     How many sessions started between <see cref="Timestamp"/> 
        ///     and <see cref="Duration"/> seconds after <see cref="Timestamp"/>
        /// </summary>
        public int Logins { get; set; }

        /// <summary>
        ///     How many sessions concluded between <see cref="Timestamp"/> 
        ///     and <see cref="Duration"/> seconds after <see cref="Timestamp"/>
        /// </summary>
        public int Logouts { get; set; }

        /// <summary>
        ///     How unique characters had sessions that took place between <see cref="Timestamp"/>
        ///     and <see cref="Duration"/> seconds after <see cref="Timestamp"/>
        /// </summary>
        public int UniqueCharacters { get; set; }

        /// <summary>
        ///     How many seconds of playtime occured between <see cref="Timestamp"/>
        ///     and <see cref="Duration"/> seconds after <see cref="Timestamp"/>
        /// </summary>
        public long SecondsPlayed { get; set; }

        /// <summary>
        ///     How many seconds the average session was durint this duration
        /// </summary>
        public int AverageSessionLength { get; set; }

    }
}
