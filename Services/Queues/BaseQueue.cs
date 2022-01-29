using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace watchtower.Services.Queues {

    public class BaseQueue<T> {
        
        internal ConcurrentQueue<T> _Items = new ConcurrentQueue<T>();

        internal SemaphoreSlim _Signal = new SemaphoreSlim(0);

        /// <summary>
        ///     Get the next item in the list. This will block until there is one available
        /// </summary>
        /// <param name="cancel">Stopping token</param>
        public async Task<T> Dequeue(CancellationToken cancel) {
            await _Signal.WaitAsync(cancel);
            _Items.TryDequeue(out T? entry);

            return entry!;
        }

        public void QueueAtFront(T entry) {
            lock (_Items) {
                T[] items = _Items.ToArray();
                _Items.Clear();
                _Items.Enqueue(entry);
                foreach (T iter in items) {
                    _Items.Enqueue(iter);
                }
            }
            _Signal.Release();
        }

        /// <summary>
        ///     Queue a new entry into the queue
        /// </summary>
        public void Queue(T entry) {
            _Items.Enqueue(entry);
            _Signal.Release();
        }

        /// <summary>
        ///     Get how many entries are in the queue
        /// </summary>
        /// <returns></returns>
        public int Count() {
            return _Items.Count;
        }

    }
}
