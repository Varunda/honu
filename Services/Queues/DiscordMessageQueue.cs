using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace watchtower.Services.Implementations {

    public class DiscordMessageQueue : IDiscordMessageQueue {

        private ConcurrentQueue<string> _Items = new ConcurrentQueue<string>();

        private SemaphoreSlim _Signal = new SemaphoreSlim(0);

        public async Task<string> DequeueAsync(CancellationToken cancel) {
            await _Signal.WaitAsync(cancel);
            _Items.TryDequeue(out string? message);

            return message!;
        }

        public void Queue(string message) {
            if (message == null) {
                throw new ArgumentNullException(nameof(message));
            }

            _Items.Enqueue(message);
            _Signal.Release();
        }

        public int Count() {
            return _Items.Count;
        }

    }
}
