using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace watchtower.Realtime {
    public class HostedRealtimeMonitor : IHostedService {

        private readonly RealtimeMonitor _Monitor;

        public HostedRealtimeMonitor(RealtimeMonitor mon) {
            _Monitor = mon;
        }

        public Task StartAsync(CancellationToken cancel) {
            return _Monitor.OnStartAsync(cancel);
        }

        public Task StopAsync(CancellationToken cancel) {
            return _Monitor.OnShutdownAsync(cancel);
        }

    }
}
