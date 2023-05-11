using System.Collections.Generic;
using watchtower.Models.Events;

namespace watchtower.Models.Queues {

    public class FacilityControlEventQueueEntry {

        public FacilityControlEventQueueEntry(FacilityControlEvent ev, List<PlayerControlEvent> participants) {
            this.Event = ev;
            this.Participants = participants;
        }

        public FacilityControlEvent Event { get; private set; } 

        public List<PlayerControlEvent> Participants { get; private set; }

    }
}
