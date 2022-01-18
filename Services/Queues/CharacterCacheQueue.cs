using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Models.Queues;

namespace watchtower.Services.Implementations {

    public class CharacterCacheQueue : IBackgroundCharacterCacheQueue {

        private ConcurrentQueue<CharacterFetchQueueEntry> _Items = new ConcurrentQueue<CharacterFetchQueueEntry>();

        private SemaphoreSlim _Signal = new SemaphoreSlim(0);

        public void Queue(string payload) {
            if (payload == null) {
                throw new ArgumentNullException(nameof(payload));
            }

            _Items.Enqueue(new CharacterFetchQueueEntry() {
                CharacterID = payload,
                Store = true
            });
            _Signal.Release();
        }

        public void Queue(CharacterFetchQueueEntry entry) {
            _Items.Enqueue(entry);
            _Signal.Release();
        }

        public async Task<CharacterFetchQueueEntry> DequeueAsync(CancellationToken cancel) {
            await _Signal.WaitAsync(cancel);
            _Items.TryDequeue(out CharacterFetchQueueEntry? token);

            return token!;
        }

        public int Count() {
            return _Items.Count;
        }

    }
}
