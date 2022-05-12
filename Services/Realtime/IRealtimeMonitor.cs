using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace watchtower.Realtime {

    public interface IRealtimeMonitor {

        Task OnStartAsync(CancellationToken cancel);

        Task OnShutdownAsync(CancellationToken cancel);

        Task Resubscribe();

        Task Reconnect();

    }
}
