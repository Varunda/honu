using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Models.Census {

    public class CharacterItem {

        /// <summary>
        ///     ID of the character this entry is for
        /// </summary>
        public string CharacterID { get; set; } = "";

        /// <summary>
        ///     ID of the item
        /// </summary>
        public string ItemID { get; set; } = "";

        /// <summary>
        ///     If this item is an account wide unlock or not
        /// </summary>
        public bool AccountLevel { get; set; }

        /// <summary>
        ///     Field on some of the entries. The stack count of it?
        /// </summary>
        public int? StackCount { get; set; }

    }
}
