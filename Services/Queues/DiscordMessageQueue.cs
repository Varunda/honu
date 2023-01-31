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
    public class DiscordMessageQueue : BaseQueue<HonuDiscordMessage> {

        public new void Queue(HonuDiscordMessage msg) {
            if ((msg.ChannelID == null || msg.ChannelID == 0)
                && (msg.GuildID == null || msg.GuildID == 0)
                && (msg.TargetUserID == 0 || msg.TargetUserID == 0)) {

                throw new ArgumentException($"No valid target for message given. You must specify a ChannelID and GuildID, or a TargetUserID");
            }

            _Items.Enqueue(msg);
            _Signal.Release();
        }

    }

}
