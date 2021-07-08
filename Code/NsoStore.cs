using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models;

namespace watchtower.Code {

    public class NsoStore {

        private static NsoStore _Instance = new NsoStore();
        public static NsoStore Get() { return NsoStore._Instance; }

        private ConcurrentDictionary<string, NsoTrackingEntry> Entries = new ConcurrentDictionary<string, NsoTrackingEntry>();

    }
}
