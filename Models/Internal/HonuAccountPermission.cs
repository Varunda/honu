using System;

namespace watchtower.Models {

    /// <summary>
    ///     Represents a grant to let a user perform a protected action
    /// </summary>
    public class HonuAccountPermission {

        /// <summary>
        ///     Unique ID of the permission
        /// </summary>
        public long ID { get; set; }

        /// <summary>
        ///     What <see cref="HonuAccount"/> this permission is granted to
        /// </summary>
        public long AccountID { get; set; }

        /// <summary>
        ///     What the permission is
        /// </summary>
        public string Permission { get; set; } = "";

        /// <summary>
        ///     When this permission was added
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        ///     What <see cref="HonuAccount"/> granted this permission
        /// </summary>
        public long GrantedByID { get; set; }

    }
}
