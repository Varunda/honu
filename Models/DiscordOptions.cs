using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Models {

    public class DiscordOptions {

        public bool Enabled { get; set; } = false;

        public ulong ChannelId { get; set; }

        public string Key { get; set; } = "aaa";

    }
}
