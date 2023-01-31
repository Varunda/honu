using DSharpPlus.Entities;
using System.Collections.Generic;

namespace watchtower.Models.Discord {

    /// <summary>
    ///     Wrapper around whatever Discord library being used
    /// </summary>
    public class HonuDiscordMessage {

        public enum TargetType {
            INVALID,

            CHANNEL,

            USER
        }

        /// <summary>
        ///     nullary ctor
        /// </summary>
        public HonuDiscordMessage() {

        }

        /// <summary>
        ///     copy ctor
        /// </summary>
        /// <param name="other"></param>
        public HonuDiscordMessage(HonuDiscordMessage other) {
            GuildID = other.GuildID;
            ChannelID = other.ChannelID;
            TargetUserID = other.TargetUserID;
            Message = other.Message;
            Embeds = new List<DSharpPlus.Entities.DiscordEmbed>(other.Embeds);
            Mentions = new List<IMention>(other.Mentions);
        }

        public ulong? GuildID { get; set; }

        public ulong? ChannelID { get; set; }

        public ulong? TargetUserID { get; set; }

        /// <summary>
        ///     Message to be sent. If you want to send an embedded message instead, populate <see cref="Embeds"/>
        /// </summary>
        public string Message { get; set; } = "";

        /// <summary>
        ///     Any embeds to use in the message. Leave empty to instead send <see cref="Message"/> as plain text
        /// </summary>
        public List<DSharpPlus.Entities.DiscordEmbed> Embeds { get; set; } = new();

        /// <summary>
        ///     Get the mentions this message contains
        /// </summary>
        public List<IMention> Mentions { get; set; } = new();

        /// <summary>
        ///     Get the <see cref="TargetType"/> of this message
        /// </summary>
        public TargetType Type { 
            get {
                if (TargetUserID != null) {
                    return TargetType.USER;
                }
                if (GuildID != null && ChannelID != null) {
                    return TargetType.CHANNEL;
                }

                return TargetType.INVALID;
            }
        }

    }

}
