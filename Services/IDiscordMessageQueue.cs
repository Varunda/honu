using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace watchtower.Services {

    public interface IDiscordMessageQueue {

        void Queue(string message);

        Task<string> DequeueAsync(CancellationToken cancel);

        int Count();

    }

}
