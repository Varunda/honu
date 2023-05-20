using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace watchtower.Services.Queues {

    public class BaseQueue<T> : IProcessQueue {

        internal ILogger _Logger;

        internal ConcurrentQueue<T> _Items = new ConcurrentQueue<T>();

        internal SemaphoreSlim _Signal = new SemaphoreSlim(0);

        internal ConcurrentQueue<long> _ProcessTime = new ConcurrentQueue<long>();

        internal long _ProcessedCount = 0;

        public BaseQueue(ILoggerFactory factory) {
            _Logger = factory.CreateLogger($"watchtower.Services.Queues.BaseQueue<{typeof(T).Name}>");
        }

        /// <summary>
        ///     Get the next item in the list. This will block until there is one available
        /// </summary>
        /// <param name="cancel">Stopping token</param>
        public async Task<T> Dequeue(CancellationToken cancel) {
            await _Signal.WaitAsync(cancel);
            _Items.TryDequeue(out T? entry);
            ++_ProcessedCount;

            return entry!;
        }

        /// <summary>
        ///     Insert a new entry into the front of the queue. Use this sparingly,
        ///     as in order to insert at the top of the list, a copy of the list must be allocated,
        ///     the items are cleared, then each item is re-queued behind <paramref name="entry"/>
        /// </summary>
        /// <param name="entry">Entry to be queued at the front</param>
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
        ///     Add some basic metrics about how long it took to process "something" in the queue
        /// </summary>
        /// <param name="ms">How many milliseconds it took to process something that came from this queue</param>
        public void AddProcessTime(long ms) {
            _ProcessTime.Enqueue(ms);
            while (_ProcessTime.Count > 100) {
                _ = _ProcessTime.TryDequeue(out _);
            }
        }

        /// <summary>
        ///     Get a copy of a list that contains how many milliseconds it took to process data in this queue
        /// </summary>
        /// <returns>
        ///     A newly allocated list whose elements represent how long it took to process data from this queue
        /// </returns>
        public List<long> GetProcessTime() {
            lock (_ProcessTime) {
                List<long> ms = new List<long>(_ProcessTime);
                return ms;
            }
        }

        /// <summary>
        ///     Get how many entries are in the queue
        /// </summary>
        /// <returns></returns>
        public int Count() {
            return _Items.Count;
        }

        /// <summary>
        ///     Return how many items have been removed from this queue
        /// </summary>
        /// <returns></returns>
        public long Processed() {
            return _ProcessedCount;
        }

        /// <summary>
        ///     Allocate a copy of the items in the list
        /// </summary>
        /// <returns>A newly allocated list that contains a shallow-reference to the items in the list</returns>
        public List<T> ToList() {
            T[] arr = new T[Count()];
            _Items.CopyTo(arr, 0);
            return arr.ToList();
        }

    }
}
