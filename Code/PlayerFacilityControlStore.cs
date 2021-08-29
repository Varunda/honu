using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models.Events;

namespace watchtower.Code {

    public class PlayerFacilityControlStore {

        private static PlayerFacilityControlStore _Instance = new PlayerFacilityControlStore();
        public static PlayerFacilityControlStore Get() { return _Instance; }

        public List<PlayerControlEvent> Events { get; } = new List<PlayerControlEvent>();

    }
}
