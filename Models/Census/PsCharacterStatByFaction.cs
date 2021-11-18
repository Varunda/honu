using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Models.Census {
    public class PsCharacterStatByFaction {

        public string CharacterID { get; set; } = "";

        public string StatName { get; set; } = "";

        public int ProfileID { get; set; }

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

        public DateTime Timestamp { get; set; }

    }
}
