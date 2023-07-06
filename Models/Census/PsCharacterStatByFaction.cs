using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Code.Constants;

namespace watchtower.Models.Census {

    /// <summary>
    ///     Wrapper around /characters_stat_by_faction
    /// </summary>
    public class PsCharacterStatByFaction {

        /// <summary>
        ///     ID of the character this stat is for
        /// </summary>
        public string CharacterID { get; set; } = "";

        /// <summary>
        ///     Name of this stat
        /// </summary>
        public string StatName { get; set; } = "";

        /// <summary>
        ///     ID of the profile/class. See <see cref="Profile"/>
        /// </summary>
        public int ProfileID { get; set; }

        /// <summary>
        ///     When this value was last updated
        /// </summary>
        public DateTime Timestamp { get; set; }

        public int ValueForeverVS { get; set; }
        public int ValueMonthlyVS { get; set; }
        public int ValueWeeklyVS { get; set; }
        public int ValueDailyVS { get; set; }
        public int ValueOneLifeMaxVS { get; set; }

        public int ValueForeverNC { get; set; }
        public int ValueMonthlyNC { get; set; }
        public int ValueWeeklyNC { get; set; }
        public int ValueDailyNC { get; set; }
        public int ValueOneLifeMaxNC { get; set; }

        public int ValueForeverTR { get; set; }
        public int ValueMonthlyTR { get; set; }
        public int ValueWeeklyTR { get; set; }
        public int ValueDailyTR { get; set; }
        public int ValueOneLifeMaxTR { get; set; }


    }
}
