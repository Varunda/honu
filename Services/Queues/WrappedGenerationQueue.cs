using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Models.Wrapped;

namespace watchtower.Services.Queues {

    public class WrappedGenerationQueue : BaseQueue<WrappedEntry> {

        private readonly HashSet<Guid> _Pending = new();

        private Dictionary<Guid, int> _QueuePosition = new();

        public WrappedGenerationQueue(ILoggerFactory factory) : base(factory) { }

        public new void Queue(WrappedEntry entry) {
            lock (_Pending) {
                if (_Pending.Contains(entry.ID)) {
                    _Logger.LogDebug($"{entry.ID} already in queue, not doing it");
                    return;
                }

                _Pending.Add(entry.ID);
            }

            lock (_QueuePosition) {
                _QueuePosition[entry.ID] = _QueuePosition.Count;
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

            lock (_QueuePosition) {
                _QueuePosition.Remove(entry.ID);
                foreach (Guid key in _QueuePosition.Keys) {
                    _QueuePosition[key] = _QueuePosition[key] - 1;
                }
            }

            return entry;
        }

        public int GetQueuePosition(Guid id) {
            lock (_QueuePosition) {
                if (_QueuePosition.TryGetValue(id, out int pos) == true) {
                    return pos;
                }
            }
            return -1;
        }

        public Dictionary<Guid, int> GetQueuePositions() {
            lock (_QueuePosition) {
                return new Dictionary<Guid, int>(_QueuePosition);
            }
        }

    }
}
