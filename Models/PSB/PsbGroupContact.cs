using System.Collections.Generic;
using System.Linq;

namespace watchtower.Models.PSB {

    /// <summary>
    ///     a <see cref="PsbContact"/> that can act on behalf of groups
    /// </summary>
    public class PsbGroupContact : PsbContact {

        public enum RepresentativeType {
            DEFAULT,

            OVO,

            COMMUNITY,

            OBSERVER,

            PRACTICE
        }

        /// <summary>
        ///     what type of representation this contact is for
        /// </summary>
        public RepresentativeType RepType { get; set; } = RepresentativeType.DEFAULT;

        /// <summary>
        ///     list of groups (usually an outfit tag), that a contact can act on behalf of
        /// </summary>
        public List<string> Groups { get; set; } = new();

        /// <summary>
        ///     check if a contact is a rep for a group
        /// </summary>
        /// <param name="group">name of the group</param>
        public bool IsRepFor(string group) {
            return Groups.FirstOrDefault(iter => iter.ToLower() == group.ToLower().Trim()) != null;
        }

    }
}
