using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Services.Metrics;

namespace watchtower.Services.Queues {

    /// <summary>
    ///     Queue for events from Census' realtime sockets
    /// </summary>
    public class CensusRealtimeEventQueue : BaseQueue<JToken> {

        public CensusRealtimeEventQueue(ILoggerFactory factory, QueueMetric metrics) : base(factory, metrics) {
            _QueueName = "event";
        }

    }

}
