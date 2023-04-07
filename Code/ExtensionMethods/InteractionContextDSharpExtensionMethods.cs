using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System.Collections.Generic;
using System.Threading.Tasks;
using watchtower.Models;
using watchtower.Services;

namespace watchtower.Code.ExtensionMethods {

    /// <summary>
    ///     helper extensions to make responses easier
    /// </summary>
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
                new DiscordInteractionResponseBuilder().WithContent(message).AsEphemeral(ephemeral)
            );
        }

        /// <summary>
        ///     Create a deferred response that will show a thinking indicator
        /// </summary>
        /// <param name="ctx">Extension instance</param>
        /// <param name="ephemeral">Will the response only be shown to the person who used the command?</param>
        public static Task CreateDeferred(this BaseContext ctx, bool ephemeral = false) {
            return ctx.CreateResponseAsync(
                InteractionResponseType.DeferredChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().AsEphemeral(ephemeral)
            );
        }

        /// <summary>
        ///     Edit a response with an embed
        /// </summary>
        /// <param name="ctx">extension instance</param>
        /// <param name="embed">embed to edit into the response</param>
        public static Task EditResponseEmbed(this BaseContext ctx, DiscordEmbed embed) {
            return ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
        }

        /// <summary>
        ///     Edit a deferred response started with <see cref="CreateDeferred(BaseContext, bool)"/>
        ///     with a text message
        /// </summary>
        /// <param name="ctx">Extension instance</param>
        /// <param name="message">Message to send</param>
        public static Task EditResponseText(this BaseContext ctx, string message) {
            return ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(message));
        }

        /// <summary>
        ///     Create an text response to an interaction and send it
        /// </summary>
        /// <param name="ctx">Extension instance</param>
        /// <param name="message">Message to be send</param>
        /// <param name="ephemeral">Will this message be dismissable?</param>
        public static Task CreateImmediateText(this DiscordInteraction ctx, string message, bool ephemeral = false) {
            return ctx.CreateResponseAsync(
                InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().WithContent(message).AsEphemeral(ephemeral)
            );
        }

        /// <summary>
        ///     Create a deferred response that will show a thinking indicator
        /// </summary>
        /// <param name="ctx">Extension instance</param>
        /// <param name="ephemeral">Will this message be dismissable?</param>
        public static Task CreateDeferred(this DiscordInteraction ctx, bool ephemeral = false) {
            return ctx.CreateResponseAsync(
                InteractionResponseType.DeferredChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().AsEphemeral(ephemeral)
            );
        }

        /// <summary>
        ///     Create a deferred response to a component interaction
        /// </summary>
        /// <param name="ctx">extension instance</param>
        /// <param name="ephemeral">will this response be ephemeral?</param>
        public static Task CreateComponentDeferred(this DiscordInteraction ctx, bool ephemeral = true) {
            return ctx.CreateResponseAsync(
                InteractionResponseType.DeferredMessageUpdate,
                new DiscordInteractionResponseBuilder().AsEphemeral(ephemeral)
            );
        }

        /// <summary>
        ///     Create an immediate response with an embed builder
        /// </summary>
        /// <param name="ctx">Extension instance</param>
        /// <param name="builder">Embed builder</param>
        /// <param name="ephemeral">Will this message be dismissable?</param>
        public static Task CreateImmediateEmbed(this DiscordInteraction ctx, DiscordEmbedBuilder builder, bool ephemeral = false) {
            return ctx.CreateResponseAsync(
                InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().AddEmbed(builder).AsEphemeral(ephemeral)
            );
        }

        /// <summary>
        ///     Edit a deferred response started with <see cref="CreateDeferred(DiscordInteraction, bool)"/>
        ///     with an embed response
        /// </summary>
        /// <param name="ctx">Extension instance</param>
        /// <param name="builder">Embed builder</param>
        public static Task EditResponseEmbed(this DiscordInteraction ctx, DiscordEmbedBuilder builder) {
            return ctx.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddEmbed(builder));
        }

        /// <summary>
        ///     Edit a response with a list of embeds
        /// </summary>
        /// <param name="ctx">extension instance</param>
        /// <param name="embeds">embeds to edit into the response</param>
        public static Task EditResponseEmbed(this DiscordInteraction ctx, params DiscordEmbed[] embeds) {
            return ctx.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddEmbeds(embeds));
        }

        /// <summary>
        ///     Edit a response with a list of embeds
        /// </summary>
        /// <param name="ctx">extension instance</param>
        /// <param name="embeds">embeds to edit into the response</param>
        public static Task EditResponseEmbed(this DiscordInteraction ctx, IReadOnlyList<DiscordEmbed> embeds) {
            return ctx.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddEmbeds(embeds));
        }

        /// <summary>
        ///     Edit a deferred response started with <see cref="CreateDeferred(DiscordInteraction, bool)"/>
        ///     with a text message response
        /// </summary>
        /// <param name="ctx">Extension instance</param>
        /// <param name="message">Text message to be sent</param>
        public static Task EditResponseText(this DiscordInteraction ctx, string message) {
            return ctx.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent(message));
        }

        /// <summary>
        ///     Edit a deferred response with an error message as an embed
        /// </summary>
        /// <param name="ctx">extension instance</param>
        /// <param name="error">description of the error, goes into the description of the embed</param>
        /// <param name="title">optional title, defaults to "Error"</param>
        public static Task EditResponseErrorEmbed(this DiscordInteraction ctx, string error, string? title = null) {
            return ctx.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddEmbed(
                new DiscordEmbedBuilder()
                    .WithTitle(title ?? "Error")
                    .WithDescription(error)
                    .WithColor(DiscordColor.Red)
                )
            );
        }

    }
}
