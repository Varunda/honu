using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models;

namespace watchtower.Code {

    public class ConnectionStore {

        private static ConnectionStore _Instance = new ConnectionStore();
        public static ConnectionStore Get() { return ConnectionStore._Instance; }

        public ConcurrentDictionary<string, TrackedConnection> Connections = new ConcurrentDictionary<string, TrackedConnection>();

    }
}
