using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace watchtower.Services {

    public class BackgroundTaskQueue : IBackgroundTaskQueue {

        private ConcurrentQueue<JToken> _Items = new ConcurrentQueue<JToken>();

        private SemaphoreSlim _Signal = new SemaphoreSlim(0);

        public void Queue(JToken payload) {
            if (payload == null) {
                throw new ArgumentNullException(nameof(payload));
            }

            _Items.Enqueue(payload);
            _Signal.Release();
        }

        public async Task<JToken> DequeueAsync(CancellationToken cancel) {
            await _Signal.WaitAsync(cancel);
            _Items.TryDequeue(out JToken? token);

            return token!;
        }

    }
}
