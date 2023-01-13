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
        public static Task CreateImmediateText(this InteractionContext ctx, string message) {
            return ctx.CreateResponseAsync(
                InteractionResponseType.ChannelMessageWithSource,
                new DSharpPlus.Entities.DiscordInteractionResponseBuilder().WithContent(message)
            );
        }

        /// <summary>
        ///     Create a <see cref="InteractionResponseType.DeferredChannelMessageWithSource"/> response with a message.
        ///     Use <see cref="EditResponseText(InteractionContext, string)"/> to edit the response
        /// </summary>
        /// <param name="ctx">Extension instance</param>
        /// <param name="message">Message to send</param>
        public static Task CreateDeferredText(this InteractionContext ctx, string message) {
            return ctx.CreateResponseAsync(
                InteractionResponseType.DeferredChannelMessageWithSource,
                new DSharpPlus.Entities.DiscordInteractionResponseBuilder().WithContent(message)
            );
        }

        /// <summary>
        ///     Helper method around
        ///     <see cref="BaseContext.EditResponseAsync(DSharpPlus.Entities.DiscordWebhookBuilder, System.Collections.Generic.IEnumerable{DSharpPlus.Entities.DiscordAttachment})"/>
        /// </summary>
        /// <param name="ctx">Extension instance</param>
        /// <param name="message">Message to send</param>
        public static Task EditResponseText(this InteractionContext ctx, string message) {
            return ctx.EditResponseAsync(new DSharpPlus.Entities.DiscordWebhookBuilder().WithContent(message));
        }

    }
}
