using DSharpPlus.ButtonCommands;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Code.Constants;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Census;
using watchtower.Models.Db;
using watchtower.Services.Db;
using watchtower.Services.Repositories;

namespace watchtower.Code.DiscordInteractions {

    [SlashCommandGroup("subscribe", "Subscribe to events")]
    public class SubscribeSlashCommand : PermissionSlashCommand {

        public ILogger<SubscribeSlashCommand> _Logger { set; private get; } = default!;

        public CharacterRepository _CharacterRepository { set; private get; } = default!;
        public SessionEndSubscriptionDbStore _SubscriptionDb { set; private get; } = default!;

        /// <summary>
        ///     Create a session end subscription
        /// </summary>
        /// <param name="ctx">Provided context</param>
        /// <param name="name">Name of the character to create the subscription for</param>
        [SlashCommand("character", "Have the bot send you a link in a DM when a session ends")]
        public async Task SubscribeLogoutCommand(InteractionContext ctx,
            [Option("Character", "Name of the character")] string name) {

            await ctx.CreateDeferred(true);

            PsCharacter? c = await _CharacterRepository.GetFirstByName(name);
            if (c == null) {
                DiscordWebhookBuilder fInteractionBuilder = new();
                DiscordEmbedBuilder fBuilder = new();

                fBuilder.Color = DiscordColor.Red;
                fBuilder.Title = $"Subscription failed!";
                fBuilder.Description = $"No character named `{name}` exists";
                fBuilder.Timestamp = DateTimeOffset.UtcNow;

                fInteractionBuilder.AddEmbed(fBuilder);

                await ctx.EditResponseAsync(fInteractionBuilder);
                return;
            }

            List<SessionEndSubscription> existingSubs = await _SubscriptionDb.GetByDiscordID(ctx.User.Id);
            SessionEndSubscription? existingSub = existingSubs.FirstOrDefault(iter => iter.CharacterID == c.ID);

            if (existingSub == null) {
                SessionEndSubscription sub = new();
                sub.DiscordID = ctx.User.Id;
                sub.CharacterID = c.ID;

                long id = await _SubscriptionDb.Insert(sub);
                _Logger.LogDebug($"Created session end subscription for {sub.DiscordID} on character {sub.CharacterID} (id {id})");
            } else {
                _Logger.LogDebug($"Session end subscription for {existingSub.DiscordID} on character {existingSub.CharacterID} already exists {existingSub.ID}");
            }

            DiscordWebhookBuilder interactionBuilder = new();
            DiscordEmbedBuilder builder = new();

            builder.Title = $"Subscription created";
            builder.Description = $"Whenever `{c.Name}` signs out, you will be sent a DM containing a link to the session";
            builder.Timestamp = DateTimeOffset.UtcNow;
            builder.Color = DiscordColor.Green;
            interactionBuilder.AddEmbed(builder);

            await ctx.EditResponseAsync(interactionBuilder);
        }

        /// <summary>
        ///     List all the subscriptions a Discord use has
        /// </summary>
        /// <param name="ctx">Provided context</param>
        [SlashCommand("list", "List subscriptions Honu has for your Discord user")]
        public async Task SubscriptionList(InteractionContext ctx) {
            await ctx.CreateDeferred(true);

            List<SessionEndSubscription> subs = await _SubscriptionDb.GetByDiscordID(ctx.User.Id);
            List<PsCharacter> chars;
            if (subs.Count > 0) { 
                chars = await _CharacterRepository.GetByIDs(subs.Select(iter => iter.CharacterID).ToList(), CensusEnvironment.PC);
            } else {
                chars = new();
            }

            DiscordWebhookBuilder interactionBuilder = new();
            DiscordEmbedBuilder builder = new();

            builder.Title = $"Subscriptions ({subs.Count})";
            builder.Description = $"Here are the subscriptions Honu has for your Discord user";

            foreach (SessionEndSubscription sub in subs) {
                PsCharacter? c = chars.FirstOrDefault(iter => iter.ID == sub.CharacterID);
                builder.AddField("Character", $"{c?.GetDisplayName() ?? $"missing {sub.CharacterID}"}", inline: true);
            }

            interactionBuilder.AddEmbed(builder);
            await ctx.EditResponseAsync(interactionBuilder);
        }

        /// <summary>
        ///     Remove a subscriptions
        /// </summary>
        /// <param name="ctx">Provided context</param>
        /// <param name="name">Name of the character to unsubscribe from</param>
        [SlashCommand("remove", "Remove a subscription")]
        public async Task SubscriptionRemove(InteractionContext ctx,
            [Option("character", "Character name")] string name) {

            name = name.Trim();

            await ctx.CreateImmediateText($"Loading...", true);

            List<PsCharacter> removed = new();

            // it's possible the character was deleted and remade, changing the ID
            // but since the name is provided, we have to assume any character with the matching
            //      name is to be removed, as there might be 2 characters with the same name
            List<SessionEndSubscription> subs = await _SubscriptionDb.GetByDiscordID(ctx.User.Id);
            List<PsCharacter> chars = await _CharacterRepository.GetByName(name);
            _Logger.LogDebug($"Removing '{name}' from {ctx.User.GetDisplay()} ({ctx.User.Id}): have {chars.Count} characters that match");
            foreach (PsCharacter c in chars) {
                SessionEndSubscription? sub = subs.FirstOrDefault(iter => iter.CharacterID == c.ID);

                if (sub != null) {
                    await _SubscriptionDb.DeleteByID(sub.ID);
                    _Logger.LogTrace($"Removed ID {sub.ID} for {c.GetDisplayName()} ({c.ID}) for discord user {ctx.User.Id}");
                    removed.Add(c);
                }
            }

            _Logger.LogDebug($"Removed {removed.Count} characters from {ctx.User.GetDisplay()} ({ctx.User.Id}) with name '{name}'");

            DiscordWebhookBuilder interactionBuilder = new();
            DiscordEmbedBuilder builder = new();

            if (removed.Count > 0) {
                builder.Title = $"Subscription removed";
                builder.Description = $"You will no longer get DMs when {string.Join(", ", removed.Select(iter => $"`{iter.Name}`"))} ends a session";
                builder.Color = DiscordColor.Green;
            } else {
                builder.Title = $"Subscription not found!";
                builder.Description = $"You are not subscribed to `{name}`. Cannot remove subscription.\nUse `/subscribe list` to list your subscriptions";
                builder.Color = DiscordColor.Red;
            }

            interactionBuilder.AddComponents(SubscribeButtonCommands.LIST_SUBS());
            interactionBuilder.AddEmbed(builder);
            await ctx.EditResponseAsync(interactionBuilder);
        }

    }

    public class SubscribeButtonCommands : ButtonCommandModule {

        /// <summary>
        ///     Create button to list the sessions
        /// </summary>
        /// <returns></returns>
        public static DiscordButtonComponent LIST_SUBS() => new(DSharpPlus.ButtonStyle.Secondary, "@sub-list", "List subscriptions");

        /// <summary>
        ///     Create a button to remove a user's subscription to a character session end
        /// </summary>
        /// <param name="charID"></param>
        /// <returns></returns>
        public static DiscordButtonComponent REMOVE_CHAR_SUB(string charID) => new(DSharpPlus.ButtonStyle.Danger, $"@sub-char-remove.{charID}", "Unsubscribe");

        public ILogger<SubscribeButtonCommands> _Logger { set; private get; } = default!;

        public CharacterRepository _CharacterRepository { set; private get; } = default!;
        public SessionEndSubscriptionDbStore _SubscriptionDb { set; private get; } = default!;

        [ButtonCommand("sub-char-remove")]
        public async Task UnsubscribeSessionEnd(ButtonContext ctx, string charID) {
            await ctx.Interaction.CreateDeferred(true);

            PsCharacter? c = await _CharacterRepository.GetByID(charID, CensusEnvironment.PC);
            List<SessionEndSubscription> subs = await _SubscriptionDb.GetByDiscordID(ctx.User.Id);
            SessionEndSubscription? sub = subs.FirstOrDefault(iter => iter.CharacterID == charID);

            if (sub == null) {
                DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
                    .WithTitle("Subscription not found")
                    .WithColor(DiscordColor.Red)
                    .WithDescription($"You are not subscribed to `{c?.Name}`. Cannot remove subscription.\nUse `/subscribe list` to list your subscriptions");

                DiscordWebhookBuilder webBuilder = new();
                webBuilder.AddEmbed(builder);

                webBuilder.AddComponents(LIST_SUBS());

                await ctx.Interaction.EditOriginalResponseAsync(webBuilder);
                return;
            }

            await _SubscriptionDb.DeleteByID(sub.ID);

            await ctx.Interaction.EditResponseEmbed(new DiscordEmbedBuilder()
                .WithTitle("Subscription removed")
                .WithColor(DiscordColor.Green)
                .WithDescription($"You will not longer get DMs when `{c?.Name}` ends a session")
            );
        }

        [ButtonCommand("sub-list")]
        public async Task ListSubscriptions(ButtonContext ctx) {
            await ctx.Interaction.CreateDeferred(true);

            List<SessionEndSubscription> subs = await _SubscriptionDb.GetByDiscordID(ctx.User.Id);
            List<PsCharacter> chars;
            if (subs.Count > 0) { 
                chars = await _CharacterRepository.GetByIDs(subs.Select(iter => iter.CharacterID).ToList(), CensusEnvironment.PC);
            } else {
                chars = new();
            }

            DiscordEmbedBuilder builder = new();

            builder.Title = $"Subscriptions ({subs.Count})";
            builder.Description = $"Here are the subscriptions Honu has for your Discord user";

            foreach (SessionEndSubscription sub in subs) {
                PsCharacter? c = chars.FirstOrDefault(iter => iter.ID == sub.CharacterID);
                builder.AddField("Character", $"{c?.GetDisplayName() ?? $"missing {sub.CharacterID}"}", inline: true);
            }

            await ctx.Interaction.EditResponseEmbed(builder);
        }

    }

}
