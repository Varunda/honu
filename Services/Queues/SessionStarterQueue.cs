using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Models;
using watchtower.Models.Queues;
using watchtower.Services.Metrics;

namespace watchtower.Services.Queues {

    /// <summary>
    ///     Queue for starting player sessions
    /// </summary>
    public class SessionStarterQueue : BaseQueue<CharacterSessionStartQueueEntry> {

        private new readonly ILogger<SessionStarterQueue> _Logger;

        public SessionStarterQueue(ILogger<SessionStarterQueue> logger, 
            ILoggerFactory factory, QueueMetric metrics)
                : base (factory, metrics) {

            _Logger = logger;
        }

        private readonly HashSet<string> _Pending = new HashSet<string>();

        public new void Queue(CharacterSessionStartQueueEntry entry) {
            if (entry.CharacterID == "0") {
                throw new Exception("no 0");
            }

            lock (_Pending) {
                if (_Pending.Contains(entry.CharacterID)) {
                    return;
                }
                _Pending.Add(entry.CharacterID);
            }

            base.Queue(entry);
        }

        public new async Task<CharacterSessionStartQueueEntry> Dequeue(CancellationToken cancel) {
            await _Signal.WaitAsync(cancel);
            _Items.TryDequeue(out CharacterSessionStartQueueEntry? entry);

            lock (_Pending) {
                _Pending.Remove(entry!.CharacterID);
            }

            ++_ProcessedCount;

            return entry!;
        }

    }

}
