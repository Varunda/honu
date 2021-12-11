using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Models;

namespace watchtower.Services {

    public interface IBackgroundSessionStarterQueue {

        void Queue(TrackedPlayer player);

        Task<TrackedPlayer> DequeueAsync(CancellationToken cancel);

        int Count();

    }
}
