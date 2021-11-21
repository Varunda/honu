using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace watchtower.Services.Implementations {

    public class BackgroundCharacterWeaponStatQueue : IBackgroundCharacterWeaponStatQueue {

        private ConcurrentQueue<string> _Items = new ConcurrentQueue<string>();

        private SemaphoreSlim _Signal = new SemaphoreSlim(0);

        public void Queue(string charID) {
            _Items.Enqueue(charID);
            _Signal.Release();
        }

        public async Task<string> Dequeue(CancellationToken cancel) {
            await _Signal.WaitAsync(cancel);
            _Items.TryDequeue(out string? charID);

            return charID!;
        }

        public int Count() {
            return _Items.Count();
        }

    }
}
