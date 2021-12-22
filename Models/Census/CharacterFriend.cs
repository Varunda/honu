using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Models.Census {

    public class CharacterFriend {

        /// <summary>
        ///     ID of the character this entry is for
        /// </summary>
        public string CharacterID { get; set; } = "";

        /// <summary>
        ///     ID of the character the friend is for
        /// </summary>
        public string FriendID { get; set; } = "";

    }
}
