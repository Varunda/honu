using System;

namespace watchtower.Models.Db {

    /// <summary>
    ///     When a Discord user would like to get notifications about a session ending, they will have one of these
    /// </summary>
    public class SessionEndSubscription {

        /// <summary>
        ///     Unique ID of the subscription
        /// </summary>
        public long ID { get; set; }

        /// <summary>
        ///     What Discord ID created the subscription
        /// </summary>
        public ulong DiscordID { get; set; }

        /// <summary>
        ///     What character ID is the subscription for
        /// </summary>
        public string CharacterID { get; set; } = "";

        /// <summary>
        ///     Timestamp of when this subscription was created
        /// </summary>
        public DateTime Timestamp { get; set; }

    }
}
