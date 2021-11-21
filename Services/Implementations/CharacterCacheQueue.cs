using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace watchtower.Services.Implementations {

    public class CharacterCacheQueue : IBackgroundCharacterCacheQueue {

        private ConcurrentQueue<string> _Items = new ConcurrentQueue<string>();

        private SemaphoreSlim _Signal = new SemaphoreSlim(0);

        public void Queue(string payload) {
            if (payload == null) {
                throw new ArgumentNullException(nameof(payload));
            }

            _Items.Enqueue(payload);
            _Signal.Release();
        }

        public async Task<string> DequeueAsync(CancellationToken cancel) {
            await _Signal.WaitAsync(cancel);
            _Items.TryDequeue(out string? token);

            return token!;
        }

        public int Count() {
            return _Items.Count;
        }

    }
}
