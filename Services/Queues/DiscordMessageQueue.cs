using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Models.Discord;

namespace watchtower.Services.Queues {

    /// <summary>
    ///     Queue of messages to be sent in Discord
    /// </summary>
    public class DiscordMessageQueue : BaseQueue<DiscordMessage> {

        /// <summary>
        ///     Queue a basic text discord message
        /// </summary>
        public void Queue(string message) {
            _Items.Enqueue(new DiscordMessage() {
                Message = message
            });
            _Signal.Release();
        }

    }

}
