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
        ///     ID of the VS character
        /// </summary>
        public string? VsID { get; set; }

        public int VsStatus { get; set; }

        public string? NcID { get; set; }

        public int NcStatus { get; set; }

        public string? TrID { get; set; }

        public int TrStatus { get; set; }

        public string? NsID { get; set; }

        public int NsStatus { get; set; }

    }


}
