using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Models;
using watchtower.Models.Queues;

namespace watchtower.Services.Queues {

    /// <summary>
    ///     Queue for starting player sessions
    /// </summary>
    public class SessionStarterQueue : BaseQueue<CharacterSessionStartQueueEntry> {

    }

}
