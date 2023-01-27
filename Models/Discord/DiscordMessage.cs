using System.Collections.Generic;

namespace watchtower.Models.Discord {

    /// <summary>
    ///     Wrapper around whatever Discord library being used
    /// </summary>
    public class DiscordMessage {

        /// <summary>
        ///     Any embeds to use in the message. Leave empty to instead send <see cref="Message"/> as plain text
        /// </summary>
        public List<DiscordEmbed> Embeds { get; set; } = new List<DiscordEmbed>();

        /// <summary>
        ///     Message to be sent. If you want to send an embedded message instead, populate <see cref="Embeds"/>
        /// </summary>
        public string Message { get; set; } = "";

        public List<DiscordMention> Mentions { get; set; } = new();

    }
}
