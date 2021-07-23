using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Models;

namespace watchtower.Services {

    public class BackgroundSessionStarterQueue : IBackgroundSessionStarterQueue {

        private ConcurrentQueue<TrackedPlayer> _Items = new ConcurrentQueue<TrackedPlayer>();

        private SemaphoreSlim _Signal = new SemaphoreSlim(0);

        public void Queue(TrackedPlayer player) {
            if (player == null) {
                throw new ArgumentNullException(nameof(player));
            }

            _Items.Enqueue(player);
            _Signal.Release();
        }

        public async Task<TrackedPlayer> DequeueAsync(CancellationToken cancel) {
            await _Signal.WaitAsync(cancel);
            _Items.TryDequeue(out TrackedPlayer? player);

            return player!;
        }

    }
}
