using DSharpPlus;
using DSharpPlus.SlashCommands;
using System.Threading.Tasks;
using watchtower.Models;
using watchtower.Services;

namespace watchtower.Code.ExtensionMethods {

    public static class InteractionContextDSharpExtensionMethods {

        /// <summary>
        ///     Create a <see cref="InteractionResponseType.ChannelMessageWithSource"/> response with a message
        /// </summary>
        /// <param name="ctx">Extension instance</param>
        /// <param name="message">Message to send</param>
        /// <param name="ephemeral">Will the response only be shown to the person who used the command?</param>
        public static Task CreateImmediateText(this BaseContext ctx, string message, bool ephemeral = false) {
            return ctx.CreateResponseAsync(
                InteractionResponseType.ChannelMessageWithSource,
                new DSharpPlus.Entities.DiscordInteractionResponseBuilder().WithContent(message).AsEphemeral(ephemeral)
            );
        }

        /// <summary>
        ///     Create a <see cref="InteractionResponseType.DeferredChannelMessageWithSource"/> response with a message.
        ///     Use <see cref="EditResponseText(BaseContext, string)"/> to edit the response
        /// </summary>
        /// <param name="ctx">Extension instance</param>
        /// <param name="message">Message to send</param>
        /// <param name="ephemeral">Will the response only be shown to the person who used the command?</param>
        public static Task CreateDeferredText(this BaseContext ctx, string message, bool ephemeral = false) {
            return ctx.CreateResponseAsync(
                InteractionResponseType.DeferredChannelMessageWithSource,
                new DSharpPlus.Entities.DiscordInteractionResponseBuilder().WithContent(message).AsEphemeral(ephemeral)
            );
        }

        /// <summary>
        ///     Helper method around
        ///     <see cref="BaseContext.EditResponseAsync(DSharpPlus.Entities.DiscordWebhookBuilder, System.Collections.Generic.IEnumerable{DSharpPlus.Entities.DiscordAttachment})"/>
        /// </summary>
        /// <param name="ctx">Extension instance</param>
        /// <param name="message">Message to send</param>
        public static Task EditResponseText(this BaseContext ctx, string message) {
            return ctx.EditResponseAsync(new DSharpPlus.Entities.DiscordWebhookBuilder().WithContent(message));
        }

    }
}
