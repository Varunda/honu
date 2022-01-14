using System;
using System.Collections.Generic;

namespace watchtower.Models.PSB {

    /// <summary>
    ///     Represents info about a PSB named account
    /// </summary>
    public class PsbNamedAccount {

        /// <summary>
        ///     Internal ID
        /// </summary>
        public long ID { get; set; }

        /// <summary>
        ///     Tag of the characters on the account
        /// </summary>
        public string? Tag { get; set; }

        /// <summary>
        ///     Name of the characters on the account
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        ///     Name of the player who owns the account
        /// </summary>
        public string PlayerName { get; set; } = "";

        /// <summary>
        ///     Time stamp of when the account was created
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        ///     If the account was deleted, when was it done?
        /// </summary>
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        ///     If the account was deleted, what's the honu account ID of who did it?
        /// </summary>
        public long? DeletedBy { get; set; }

        /// <summary>
        ///     ID of the VS character
        /// </summary>
        public string? VsID { get; set; }

        /// <summary>
        ///     What <see cref="PsbCharacterStatus"/> the VS character has
        /// </summary>
        public int VsStatus { get; set; }

        /// <summary>
        ///     ID of the NC character
        /// </summary>
        public string? NcID { get; set; }

        /// <summary>
        ///     What <see cref="PsbCharacterStatus"/> the NC character has
        /// </summary>
        public int NcStatus { get; set; }

        /// <summary>
        ///     ID of the TR character
        /// </summary>
        public string? TrID { get; set; }

        /// <summary>
        ///     What <see cref="PsbCharacterStatus"/> the TR character has
        /// </summary>
        public int TrStatus { get; set; }

        /// <summary>
        ///     ID of the NS character
        /// </summary>
        public string? NsID { get; set; }

        /// <summary>
        ///     What <see cref="PsbCharacterStatus"/> the NS character has
        /// </summary>
        public int NsStatus { get; set; }

    }


}
