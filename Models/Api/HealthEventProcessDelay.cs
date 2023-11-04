using System;

namespace watchtower.Models.Api {

    public class HealthEventProcessDelay {

        public DateTime MostRecentEvent { get; set; }

        public int ProcessLag { get; set; } = 0;
        
    }
}
