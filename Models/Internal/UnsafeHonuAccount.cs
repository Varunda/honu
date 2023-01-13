using System;

namespace watchtower.Models.Internal {

    /// <summary>
    ///     Contains information that might not want to be kept around (such as Discord ID)
    /// </summary>
    public class UnsafeHonuAccount {

        /// <summary>
        ///     ID of the account
        /// </summary>
        public long ID { get; set; }

        /// <summary>
        ///     Name the owner of the account is known by
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        ///     Timestamp of when the account was created
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        ///     Email of the account owner
        /// </summary>
        public string Email { get; set; } = "";

        /// <summary>
        ///     Discord name (name#NNNN) of the account owner
        /// </summary>
        public string Discord { get; set; } = "";

        /// <summary>
        ///     Discord ID of the account owner
        /// </summary>
        public ulong DiscordID { get; set; }

        /// <summary>
        ///     When this account was deleted
        /// </summary>
        public DateTime? DeletedOn { get; set; }

        /// <summary>
        ///     Who deleted this account
        /// </summary>
        public long? DeletedBy { get; set; }

    }
}
