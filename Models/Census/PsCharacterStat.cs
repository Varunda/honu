using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Models.Census {

    public class PsCharacterStat {

        public string CharacterID { get; set; } = "";

        public string StatName { get; set; } = "";

        public int ProfileID { get; set; }

        public int ValueForever { get; set; }

        public int ValueMonthly { get; set; }

        public int ValueWeekly { get; set; }

        public int ValueDaily { get; set; }

        public int ValueMaxOneLife { get; set; }

        public DateTime Timestamp { get; set; }

    }
}
