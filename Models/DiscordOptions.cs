using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Models {

    public class DiscordOptions {

        /// <summary>
        ///     If Discord features are enabled or not
        /// </summary>
        public bool Enabled { get; set; } = false;

        /// <summary>
        ///     What channel Spark will send messages to
        /// </summary>
        public ulong ChannelId { get; set; }

        /// <summary>
        ///     What Guild Spark is at "home" in
        /// </summary>
        public ulong GuildId { get; set; }

        /// <summary>
        ///     Client key
        /// </summary>
        public string Key { get; set; } = "aaa";

        /// <summary>
        ///     Will the global commands be registered globally? Or just in the test server
        /// </summary>
        public bool RegisterGlobalCommands { get; set; } = false;

        /// <summary>
        ///     How many seconds a session must be to be sent
        /// </summary>
        public int SessionEndSubscriptionDuration { get; set; } = 60 * 10;

    }
}
