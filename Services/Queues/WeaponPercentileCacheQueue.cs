using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace watchtower.Services.Queues {

    /// <summary>
    /// Queue of weapon percentile stats to generate
    /// </summary>
    public class WeaponPercentileCacheQueue : BaseQueue<string> {

        public WeaponPercentileCacheQueue(ILoggerFactory factory) : base(factory) { }

    }

}
