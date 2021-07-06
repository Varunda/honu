using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Models {

    public class CharacterStore {

        private static CharacterStore _Instance = new CharacterStore();
        public static CharacterStore Get() { return _Instance; }

        public ConcurrentDictionary<string, TrackedPlayer> Players = new ConcurrentDictionary<string, TrackedPlayer>();

    }

}
