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

        [SlashCommand("character", "Have the bot send you a link in a DM when a session ends")]
        public async Task SubscribeLogoutCommand(InteractionContext ctx,
            [Option("Character", "Name of the character")] string name) {

            await ctx.CreateDeferredText("Loading...", true);

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

        [SlashCommand("list", "List subscriptions Honu has for your Discord user")]
        public async Task SubscriptionList(InteractionContext ctx) {
            await ctx.CreateDeferredText("Loading...", true);

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

    }
}
