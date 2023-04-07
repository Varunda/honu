using System.Collections.Generic;
using System.Linq;

namespace watchtower.Models.PSB {

    public class PsbOvOContact : PsbContact {

        public enum RepresentativeType {
            DEFAULT,

            OVO,

            COMMUNITY,

            OBSERVER
        }

        /// <summary>
        ///     What group this contact is a rep for. Can be an outfit tag or a community
        /// </summary>
        public List<string> Groups { get; set; } = new();

        /// <summary>
        ///     What type of representation this contact is for
        /// </summary>
        public string RepType { get; set; } = "";

        /// <summary>
        ///     How many accounts this contact is allow to request
        /// </summary>
        public int AccountLimit { get; set; }

        public bool AccountPings { get; set; }

        public bool BasePings { get; set; }

        public string Notes { get; set; } = "";

        /// <summary>
        ///     Check if a contact is a rep for a group
        /// </summary>
        /// <param name="group">name of the group</param>
        public bool IsRepFor(string group) {
            return Groups.FirstOrDefault(iter => iter.ToLower() == group.ToLower().Trim()) != null;
        }

    }
}
