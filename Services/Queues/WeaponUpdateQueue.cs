
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Services.Metrics;

namespace watchtower.Services.Queues {

    /// <summary>
    /// Queue of weapon stats to update, such as top killers, percentiles, etc.
    /// </summary>
    public class WeaponUpdateQueue : BaseQueue<long> {

        private readonly HashSet<long> _Pending = new HashSet<long>();
        private long? _Last;

        public WeaponUpdateQueue(ILoggerFactory factory, QueueMetric metrics) : base(factory, metrics) {
            _QueueName = "weapon_update";
        }

        public new void Queue(long id) {
            lock (_Pending) {
                if (_Pending.Contains(id)) {
                    return;
                }

                _Pending.Add(id);
            }

            _Items.Enqueue(id);
            _Signal.Release();
        }

        public new async Task<long> Dequeue(CancellationToken cancel) {
            await _Signal.WaitAsync(cancel);
            _Items.TryDequeue(out long entry);

            lock (_Pending) {
                _Pending.Remove(entry);
            }

            _Last = entry;
            ++_ProcessedCount;

            return entry;
        }

        public long? GetMostRecentDequeued() {
            return _Last;
        }

    }
}
