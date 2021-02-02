using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models.Events;

namespace watchtower.Models {

    public class KillStore {

        private static KillStore _Instance = new KillStore();
        public static KillStore Get() { return KillStore._Instance; }

        public ConcurrentDictionary<string, TrackedPlayer> Players = new ConcurrentDictionary<string, TrackedPlayer>();

    }

}
