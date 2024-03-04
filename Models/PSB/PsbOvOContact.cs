using System.Collections.Generic;
using System.Linq;

namespace watchtower.Models.PSB {

    /// <summary>
    ///     a <see cref="PsbContact"/> for a group (outfit, community, scrim team, etc.)
    /// </summary>
    public class PsbOvOContact : PsbGroupContact {

        /// <summary>
        ///     how many accounts this contact is allow to request
        /// </summary>
        public int AccountLimit { get; set; }

        public bool AccountPings { get; set; }

        public bool BasePings { get; set; }

        public string Notes { get; set; } = "";

    }
}
