using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace watchtower.Services {

    public interface IBackgroundTaskQueue {

        void Queue(JToken payload);

        Task<JToken> DequeueAsync(CancellationToken cancel);

    }
}
