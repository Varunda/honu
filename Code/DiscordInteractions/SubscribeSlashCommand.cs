using DSharpPlus;
using DSharpPlus.ButtonCommands;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
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
using static watchtower.Code.DiscordInteractions.DiscordInteractionEnums;

namespace watchtower.Code.DiscordInteractions {

    /// <summary>
    ///     Parent slash command, all sub-commands will be grouped under `/slash ...`
    /// </summary>
    [SlashCommandGroup("subscribe", "Subscribe to events")]
    public class SubscribeSlashCommand : PermissionSlashCommand {

        public ILogger<SubscribeSlashCommand> _Logger { set; private get; } = default!;

        public CharacterRepository _CharacterRepository { set; private get; } = default!;
        public OutfitRepository _OutfitRepository { set; private get; } = default!;
        public SessionEndSubscriptionDbStore _SessionSubscriptionDb { set; private get; } = default!;
        public AlertEndSubscriptionDbStore _AlertEndSubscriptionDb { set; private get; } = default!;

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
                await ctx.EditResponseEmbed(new DiscordEmbedBuilder()
                    .WithTitle("Error: no character")
                    .WithDescription("No character named `{name}` exists")
                    .WithColor(DiscordColor.Red));
                return;
            }

            List<SessionEndSubscription> existingSubs = await _SessionSubscriptionDb.GetByDiscordID(ctx.User.Id);
            SessionEndSubscription? existingSub = existingSubs.FirstOrDefault(iter => iter.CharacterID == c.ID);

            if (existingSub == null) {
                SessionEndSubscription sub = new();
                sub.DiscordID = ctx.User.Id;
                sub.CharacterID = c.ID;

                long id = await _SessionSubscriptionDb.Insert(sub);
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

        public enum CharacterAlertSubscriptionDuration : int {
            [ChoiceName("No minimum")] NONE = 0,
            [ChoiceName("5 minutes")] MIN_5 = 300,
            [ChoiceName("15 minutes")] MIN_15 = 900,
            [ChoiceName("30 minutes")] MIN_30 = 1800,
            [ChoiceName("1 hour")] MIN_60 = 3600
        }

        /// <summary>
        ///     Create an alert end subscription that'll be sent whenever a specific character participates in an alert
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="name"></param>
        /// <param name="duration"></param>
        /// <param name="worldCharMin"></param>
        /// <returns></returns>
        [SlashCommand("alert-character", "Get notified about when an alert ends and a character participated")]
        public async Task CreateCharacterAlertSubscription(InteractionContext ctx,
            [Option("character", "Character name")] string name,
            [Option("minimum_playtime", "How long the character has to participate in the alert")] CharacterAlertSubscriptionDuration duration = CharacterAlertSubscriptionDuration.NONE,
            [Option("minimum_characters", "How many unique characters have to have participated in the alert")] long? worldCharMin = 0) {

            await ctx.CreateDeferred(true);

            PsCharacter? c = await _CharacterRepository.GetFirstByName(name);
            if (c == null) {
                await ctx.EditResponseEmbed(new DiscordEmbedBuilder()
                    .WithTitle("Error: no character")
                    .WithDescription($"No character named `{name}` exists")
                    .WithColor(DiscordColor.Red)
                );
                return;
            }

            AlertEndSubscription sub = new();
            sub.CreatedByID = ctx.User.Id;
            sub.CharacterID = c.ID;
            sub.CharacterMinimumSeconds = ((int?)duration) ?? 0;
            sub.WorldCharacterMinimum = worldCharMin == null ? 0 : worldCharMin.Value;

            long ID = await _AlertEndSubscriptionDb.Insert(sub);
            _Logger.LogDebug($"Created new {nameof(AlertEndSubscription)} {ID} for {ctx.User.GetDisplay()}/{ctx.User.Id}");

            await ctx.EditResponseEmbed(new DiscordEmbedBuilder()
                .WithTitle("Success")
                .WithDescription($"Whenever {c.GetDisplayName()} participates in an alert "
                    + ((sub.CharacterMinimumSeconds > 0) ? $"for at least {TimeSpan.FromSeconds(sub.CharacterMinimumSeconds).GetRelativeFormat()} " : " ")
                    + $"with at least {sub.WorldCharacterMinimum} players, you will be DMed a link to the Honu alert page"
                )
                .WithColor(DiscordColor.Green)
            );
        }

        /// <summary>
        ///     Create an alert end subscription that'll be sent whenever an alert finishes on a world
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="world"></param>
        /// <param name="worldCharMin"></param>
        /// <returns></returns>
        [SlashCommand("alert-server", "Get notified about when an alert ends on a specific server")]
        public async Task CreateWorldAlertSubscription(InteractionContext ctx,
            [Option("server", "Server")] StatusWorlds world,
            [Option("minimum_characters", "How many unique characters have to have participated in the alert")] long? worldCharMin = 0) {

            await ctx.CreateDeferred(true);

            // if this subscription is going into a channel, ensure they have the manage channel permission
            if (ctx.Channel != null && ctx.Member != null) {
                if (ctx.Member.Permissions.HasFlag(Permissions.ManageChannels) == false) {
                    await ctx.EditResponseEmbed(new DiscordEmbedBuilder()
                        .WithTitle("Error: no permission")
                        .WithDescription($"You need the `Manage Channels` permission to execute this command")
                        .WithColor(DiscordColor.Red));
                    return;
                }
            }

            AlertEndSubscription sub = new();
            sub.CreatedByID = ctx.User.Id;
            sub.WorldID = (short)world;
            sub.WorldCharacterMinimum = worldCharMin == null ? 0 : worldCharMin.Value;

            if (ctx.Member != null && ctx.Channel != null) {
                sub.ChannelID = ctx.Channel.Id;
                sub.GuildID = ctx.Channel.GuildId;
            }

            long ID = await _AlertEndSubscriptionDb.Insert(sub);
            _Logger.LogDebug($"Created new world {nameof(AlertEndSubscription)} {ID} for {ctx.User.GetDisplay()}/{ctx.User.Id}: world ID = {sub.WorldID}, source = {sub.SourceType}");

            await ctx.EditResponseEmbed(new DiscordEmbedBuilder()
                .WithTitle("Success")
                .WithDescription($"Whenever an alert on {World.GetName(sub.WorldID.Value)} finishes "
                    + (sub.WorldCharacterMinimum > 0 ? $"with at least {sub.WorldCharacterMinimum} players, " : " ")
                    + (sub.ChannelID != null ? $"a message will be posted in this channel with " : $"you will be DMed ")
                    + $"a link to the Honu alert page"
                )
                .WithColor(DiscordColor.Green)
            );
        }

        /// <summary>
        ///     Slash command to subscribe to alert ends when an outfit has players in it
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="tag"></param>
        /// <param name="outfitCharMin"></param>
        /// <param name="worldCharMin"></param>
        /// <returns></returns>
        [SlashCommand("alert-outfit", "Get notified about when alert ends when an outfit participated")]
        public async Task CreateOutfitAlertSubscription(InteractionContext ctx,
            [Option("tag", "Outfit tag")] string tag,
            [Option("outfit_minimum", "How many players in this outfit have to have participated in this alert")] long? outfitCharMin = 0,
            [Option("minimum_characters", "How many unique characters have to have participated in this alert")] long? worldCharMin = 0) {

            await ctx.CreateDeferred(true);

            // if this subscription is going into a channel, ensure they have the manage channel permission
            if (ctx.Channel != null && ctx.Member != null) {
                if (ctx.Member.Permissions.HasFlag(DSharpPlus.Permissions.ManageChannels) == false) {
                    await ctx.EditResponseEmbed(new DiscordEmbedBuilder()
                        .WithTitle("Error: no permission")
                        .WithDescription($"You need the `Manage Channels` permission to execute this command")
                        .WithColor(DiscordColor.Red));
                    return;
                }
            }

            List<PsOutfit> outfits = await _OutfitRepository.GetByTag(tag);
            if (outfits.Count == 0) {
                await ctx.EditResponseEmbed(new DiscordEmbedBuilder()
                    .WithTitle("Error: no outfit")
                    .WithDescription($"No outfit with tag `{tag}` exists")
                    .WithColor(DiscordColor.Red)
                );
                return;
            }

            PsOutfit outfit = outfits.OrderByDescending(iter => iter.DateCreated).First();

            AlertEndSubscription sub = new();
            sub.CreatedByID = ctx.User.Id;
            sub.WorldCharacterMinimum = worldCharMin == null ? 0 : worldCharMin.Value;
            sub.OutfitCharacterMinimum = outfitCharMin == null ? 1 : outfitCharMin.Value;

            if (ctx.Member != null && ctx.Channel != null) {
                sub.ChannelID = ctx.Channel.Id;
                sub.GuildID = ctx.Channel.GuildId;
            }

            sub.OutfitID = outfit.ID;

            long ID = await _AlertEndSubscriptionDb.Insert(sub);

            await ctx.EditResponseEmbed(new DiscordEmbedBuilder()
                .WithTitle("Success")
                .WithDescription($"Whenever an alert on finishes "
                    + ($"when `[{outfit.Tag}] {outfit.Name}` had at least {sub.OutfitCharacterMinimum} member(s) participate ")
                    + (sub.WorldCharacterMinimum > 0 ? $"with at least {sub.WorldCharacterMinimum} players, " : " ")
                    + (sub.ChannelID != null ? $"a message will be posted in this channel with " : $"you will be DMed ")
                    + $"a link to the Honu alert page"
                )
                .WithColor(DiscordColor.Green)
            );

        }

        /// <summary>
        ///     List all the subscriptions a Discord use has
        /// </summary>
        /// <param name="ctx">Provided context</param>
        [SlashCommand("list", "List subscriptions Honu has configured for yourself or this channel")]
        public async Task SubscriptionList(InteractionContext ctx) {
            await ctx.CreateDeferred(true);

            List<SessionEndSubscription> sessionSubs = new();
            List<AlertEndSubscription> alertSubs = new();

            if (ctx.Member != null) {
                alertSubs = await _AlertEndSubscriptionDb.GetByChannelID(ctx.Channel.Id);
            } else {
                sessionSubs = await _SessionSubscriptionDb.GetByDiscordID(ctx.User.Id);
                alertSubs = await _AlertEndSubscriptionDb.GetByCreatedID(ctx.User.Id);
            }

            List<PsCharacter> chars = new();
            if (sessionSubs.Count > 0) { 
                chars.AddRange(await _CharacterRepository.GetByIDs(sessionSubs.Select(iter => iter.CharacterID).ToList(), CensusEnvironment.PC, fast: true));
            }

            List<PsOutfit> outfits = new();
            if (alertSubs.Count > 0) {
                outfits.AddRange(await _OutfitRepository.GetByIDs(alertSubs.Where(iter => iter.OutfitID != null).Select(iter => iter.OutfitID!).ToList()));
                chars.AddRange(await _CharacterRepository.GetByIDs(
                    alertSubs.Where(iter => iter.CharacterID != null).Select(iter => iter.OutfitID!).ToList(),
                    CensusEnvironment.PC, fast: true
                ));
            }

            DiscordWebhookBuilder interactionBuilder = new();
            DiscordEmbedBuilder builder = new();

            builder.Title = $"Subscriptions ({sessionSubs.Count + alertSubs.Count})";
            if (ctx.Member == null) {
                builder.Description = $"Here are the subscriptions Honu will send in your DMs:\n";
            } else {
                builder.Description = $"Here are the subscriptions Honu will post in this channel:\n";
            }

            List<DiscordSelectComponentOption> options = new();

            foreach (SessionEndSubscription sub in sessionSubs) {
                PsCharacter? c = chars.FirstOrDefault(iter => iter.ID == sub.CharacterID);
                builder.Description += $"**Character:** {c?.GetDisplayName() ?? $"missing {sub.CharacterID}"}\n";
                options.Add(new DiscordSelectComponentOption($"Character {c?.GetDisplayName()}", $"char-{sub.ID}"));
            }

            foreach (AlertEndSubscription sub in alertSubs) {
                if (sub.SourceType == AlertEndSubscriptionSourceType.CHARACTER) {
                    PsCharacter? c = chars.FirstOrDefault(iter => iter.ID == sub.CharacterID!);
                    builder.Description += $"**Alert end (Character):** {c?.GetDisplayName()}\n";
                    options.Add(new DiscordSelectComponentOption($"Alert (Character) {c?.GetDisplayName()}", $"alert-{sub.ID}"));
                } else if (sub.SourceType == AlertEndSubscriptionSourceType.OUTFIT) {
                    PsOutfit? o = outfits.FirstOrDefault(iter => iter.ID == sub.OutfitID!);
                    builder.Description += $"**Alert end (Outfit):** {o?.Name}\n";
                    options.Add(new DiscordSelectComponentOption($"Alert (Outfit) {o?.Name}", $"alert-{sub.ID}"));
                } else if (sub.SourceType == AlertEndSubscriptionSourceType.WORLD) {
                    builder.Description += $"**Alert end (Server):** {World.GetName((short)sub.WorldID!)}\n";
                    options.Add(new DiscordSelectComponentOption($"Alert (Server) {World.GetName((short)sub.WorldID!)}", $"alert-{sub.ID}"));
                }
            }

            interactionBuilder.AddEmbed(builder);

            // only add the dropdown to remove a subscription if there are subs to remove
            //      if they are in DMs
            //     or if in a channel, only if they have manage channel perms
            if (options.Count > 0) {
                if (ctx.Member == null || (ctx.Member != null && ctx.Channel != null && ctx.Member.Permissions.HasFlag(Permissions.ManageChannels))) {
                    DiscordSelectComponent select = new("@remove-sub", "Remove subscription", options);
                    interactionBuilder.AddComponents(select);
                }
            }

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
            List<SessionEndSubscription> subs = await _SessionSubscriptionDb.GetByDiscordID(ctx.User.Id);
            List<PsCharacter> chars = await _CharacterRepository.GetByName(name);
            _Logger.LogDebug($"Removing '{name}' from {ctx.User.GetDisplay()} ({ctx.User.Id}): have {chars.Count} characters that match");
            foreach (PsCharacter c in chars) {
                SessionEndSubscription? sub = subs.FirstOrDefault(iter => iter.CharacterID == c.ID);

                if (sub != null) {
                    await _SessionSubscriptionDb.DeleteByID(sub.ID);
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
        public static DiscordButtonComponent LIST_SUBS() => new(ButtonStyle.Secondary, "@sub-list", "List subscriptions");

        /// <summary>
        ///     Create a button to remove a user's subscription to a character session end
        /// </summary>
        /// <param name="charID"></param>
        /// <returns></returns>
        public static DiscordButtonComponent REMOVE_CHAR_SUB(string charID) => new(ButtonStyle.Danger, $"@sub-char-remove.{charID}", "Unsubscribe");

        public ILogger<SubscribeButtonCommands> _Logger { set; private get; } = default!;

        public CharacterRepository _CharacterRepository { set; private get; } = default!;
        public SessionEndSubscriptionDbStore _SubscriptionDb { set; private get; } = default!;
        public AlertEndSubscriptionDbStore _AlertEndSubscriptionDb { set; private get; } = default!;

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

        [ButtonCommand("remove-sub")]
        public async Task RemoveSubscription(ButtonContext ctx) {
            await ctx.Interaction.CreateDeferred(true);

            string[] args = ctx.Values;

            if (args.Length < 1) {
                await ctx.Interaction.EditResponseErrorEmbed("has less than 1 values in response");
                return;
            }

            string[] parts = args[0].Split("-");
            if (parts.Length != 2) {
                await ctx.Interaction.EditResponseErrorEmbed($"expected 2 strings when splitting `{args[0]}` on `-`, got {parts.Length}");
                return;
            }

            if (long.TryParse(parts[1], out long subID) == false) {
                await ctx.Interaction.EditResponseErrorEmbed($"failed to parse `{parts[1]}` into a valid Int64");
                return;
            }

            string which = parts[0].ToLower();

            if (which == "char") {
                await _SubscriptionDb.DeleteByID(subID);
                await ctx.Interaction.EditResponseEmbed(new DiscordEmbedBuilder()
                    .WithTitle("Success")
                    .WithDescription($"This alert end subscription will not longer be sent")
                    .WithFooter($"debug id {subID}")
                    .WithColor(DiscordColor.Green)
                );
            } else if (which == "alert") {
                await _AlertEndSubscriptionDb.DeleteByID(subID);
                await ctx.Interaction.EditResponseEmbed(new DiscordEmbedBuilder()
                    .WithTitle("Success")
                    .WithDescription($"This alert end subscription will not longer be sent")
                    .WithFooter($"debug id {subID}")
                    .WithColor(DiscordColor.Green)
                );
            } else {
                await ctx.Interaction.EditResponseErrorEmbed($"invalid which: `{which}` (from `{args[0]}`)");
                return;
            }
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

    public class SubscribeDiscordInteractions {

        private readonly ILogger<SubscribeDiscordInteractions> _Logger;
        private readonly CharacterRepository _CharacterRepository;
        private readonly OutfitRepository _OutfitRepositiory;
        private readonly SessionEndSubscriptionDbStore _SubscriptionDb;

        public SubscribeDiscordInteractions(ILogger<SubscribeDiscordInteractions> logger,
            CharacterRepository characterRepository, OutfitRepository outfitRepositiory,
            SessionEndSubscriptionDbStore subscriptionDb) {

            _Logger = logger;

            _CharacterRepository = characterRepository;
            _OutfitRepositiory = outfitRepositiory;
            _SubscriptionDb = subscriptionDb;
        }

    }


}
