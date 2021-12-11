using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Models.Queues {

    /// <summary>
    ///     When a character logs out, the API does not reflect this right away. To keep a character update from
    ///     occuring before the data has been saved, character IDs are kept in a buffer until the last_login field
    ///     from Census matches the last login Honu tracked. When the last_login field matches (within a couple of minutes)
    ///     or is greater than the LoginTime, then we know the character has been updated
    /// </summary>
    public class LogoutBufferEntry {

        /// <summary>
        ///     ID of the character in the buffer
        /// </summary>
        public string CharacterID { get; set; } = "";

        /// <summary>
        ///     When the character last logged in. If the last_login field from Census does not match this, we
        ///     can assume it it not yet been updated, so it goes back into the buffer
        /// </summary>
        public DateTime LoginTime { get; set; }

        /// <summary>
        ///     When this entry was created
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        ///     How many times the character has not been found in the API, potentially meaning it was deleted before Census updated
        /// </summary>
        public int NotFoundCount { get; set; }

    }
}
