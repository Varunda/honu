using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Models {

    public class NpcStore {

        private static NpcStore _Instance = new NpcStore();
        public static NpcStore Get() { return _Instance; }

        public ConcurrentDictionary<string, TrackedNpc> Npcs = new ConcurrentDictionary<string, TrackedNpc>();

    }
}
