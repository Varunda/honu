using System.Collections.Generic;
using watchtower.Models.Api;

namespace watchtower.Models.Health {

    public class HonuHealth {

        public List<CensusRealtimeHealthEntry> Death { get; set; } = new List<CensusRealtimeHealthEntry>();

        public List<CensusRealtimeHealthEntry> Exp { get; set; } = new List<CensusRealtimeHealthEntry>();

        public List<ServiceQueueCount> Queues { get; set; } = new List<ServiceQueueCount>();

    }
}
