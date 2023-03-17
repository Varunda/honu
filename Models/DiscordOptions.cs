using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models.Discord;

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
        ///     What channel reservations will be posted to by users to be parsed by Spark
        /// </summary>
        public ulong ReservationChannelId { get; set; }

        /// <summary>
        ///     What channel Spark will post parsed reservations to
        /// </summary>
        public ulong ParsedChannelId { get; set; }

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
        ///     How many seconds long a ps2 session must be in to qualify to be sent to the user
        /// </summary>
        public int SessionEndSubscriptionDuration { get; set; } = 60 * 10;

    }

}
