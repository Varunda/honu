using System.Collections.Generic;

namespace watchtower.Models.Discord {

    public class DiscordMessage {

        public DiscordMessageType Type { get; set; }

        public List<DiscordEmbed> Embeds { get; set; } = new List<DiscordEmbed>();

        public string Message { get; set; } = "";

    }
}
