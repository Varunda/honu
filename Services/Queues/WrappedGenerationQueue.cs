using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Models.Wrapped;

namespace watchtower.Services.Queues {

    public class WrappedGenerationQueue : BaseQueue<WrappedEntry> {

        private readonly HashSet<Guid> _Pending = new();

        public WrappedGenerationQueue(ILoggerFactory factory) : base(factory) { }

        public new void Queue(WrappedEntry entry) {
            lock (_Pending) {
                if (_Pending.Contains(entry.ID)) {
                    return;
                }

                _Pending.Add(entry.ID);
            }

            _Items.Enqueue(entry);
            _Signal.Release();
        }

        public new async Task<WrappedEntry> Dequeue(CancellationToken cancel) {
            await _Signal.WaitAsync(cancel);
            if (_Items.TryDequeue(out WrappedEntry? entry) == false) {
                throw new Exception($"Failed to dequeue, but signal was released, this is an invalid state and indicates incorrect usage of a BaseQueue<T>");
            }

            lock (_Pending) {
                _Pending.Remove(entry.ID);
            }

            return entry;
        }

    }
}
