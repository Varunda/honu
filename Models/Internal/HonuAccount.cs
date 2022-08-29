using System;

namespace watchtower.Models {

    /// <summary>
    ///     Represents the info about an account with Honu
    /// </summary>
    public class HonuAccount {

        /// <summary>
        ///     Default ID of the system account
        /// </summary>
        public const long SystemID = 1;

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
