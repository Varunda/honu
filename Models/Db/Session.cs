using System;

namespace watchtower.Models.Db {

    /// <summary>
    /// Represents a period of time when a character played
    /// </summary>
    public class Session {

        /// <summary>
        /// Unique ID of the session
        /// </summary>
        public long ID { get; set; }

        /// <summary>
        /// ID of the character this session is for
        /// </summary>
        public string CharacterID { get; set; } = "";

        /// <summary>
        /// When this session started
        /// </summary>
        public DateTime Start { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// When this session ended. Null if currently in progress
        /// </summary>
        public DateTime? End { get; set; }

        /// <summary>
        /// What outfit the character is in, just for this session. Used for historical outfit pop
        /// </summary>
        public string? OutfitID { get; set; }

        /// <summary>
        /// What team the character is on during this session. Used for NSO
        /// </summary>
        public short TeamID { get; set; }

    }

}