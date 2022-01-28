using System.Collections.Generic;

namespace watchtower.Models.Discord {

    public class DiscordEmbed {

        public string Title { get; set; } = "";

        public string Description { get; set; } = "";

        public string Color { get; set; } = "#ffffff";

        public List<DiscordEmbedField> Fields { get; set; } = new List<DiscordEmbedField>();

    }
}
