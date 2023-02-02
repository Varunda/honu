using System.Collections.Generic;
using watchtower.Models.Census;
using watchtower.Models.Events;

namespace watchtower.Models.Api {

    public class ItemAddedEventBlock {

        public List<ItemAddedEvent> Events { get; set; } = new();

        public List<PsItem> Items { get; set; } = new();

    }
}
