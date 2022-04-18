using System;

namespace watchtower.Models {

    public class WorldTagEntry {

        /// <summary>
        ///     Unique ID of the tag entry
        /// </summary>
        public long ID { get; set; }

        /// <summary>
        ///     ID of the character who is the kill target
        /// </summary>
        public string CharacterID { get; set; } = "";

        /// <summary>
        ///     ID of the world this tag entry is for
        /// </summary>
        public short WorldID { get; set; }

        /// <summary>
        ///     Timestamp of when the kill target became the kill target
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        ///     When the kill target was killed and was no longer the kill target
        /// </summary>
        public DateTime? TargetKilled { get; set; } = null;

        /// <summary>
        ///     When the kill target last got a kill
        /// </summary>
        public DateTime LastKill { get; set; }

        /// <summary>
        ///     How many kills the kill target has gotten while as the kill target
        /// </summary>
        public int Kills { get; set; }

        /// <summary>
        ///     Was the kill target moved because the target was killed, or because they went AFK?
        /// </summary>
        public bool? WasKilled { get; set; } = null;

    }
}
