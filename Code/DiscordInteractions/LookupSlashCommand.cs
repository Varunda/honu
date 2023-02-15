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
using watchtower.Constants;
using watchtower.Models;
using watchtower.Models.Census;
using watchtower.Services.Repositories;

namespace watchtower.Code.DiscordInteractions {

    public class LookupSlashCommand : PermissionSlashCommand {

        public ILogger<LookupSlashCommand> _Logger { set; private get; } = default!;
        public CharacterRepository _CharacterRepository { set; private get; } = default!;
        public OutfitRepository _OutfitRepository { set; private get; } = default!;

        public LookupDiscordInteractions _LookupInteractions { set; private get; } = default!;

        /// <summary>
        ///     Slash command to perform a character lookup.
        ///     If multiple exist with the same name, the one most recently logged in is used
        /// </summary>
        /// <param name="ctx">Provided context</param>
        /// <param name="name">Name of the character</param>
        [SlashCommand("character", "Do a character lookup")]
        public async Task CharacterLookupCommand(InteractionContext ctx,
            [Option("name", "Character name")] string name) {

            await ctx.CreateDeferred(true);
            DiscordWebhookBuilder builder = await _LookupInteractions.CharacterLookupByName(name);
            await ctx.EditResponseAsync(builder);
        }

        /// <summary>
        ///     Slash command to perform an outfit lookup by tag
        /// </summary>
        /// <param name="ctx">Provded context</param>
        /// <param name="tag">Case-insensitive outfit tag</param>
        [SlashCommand("outfit", "Search an outfit by tag")]
        public async Task OutfitLookupCommand(InteractionContext ctx,
            [Option("tag", "Outfit tag")] string tag) {

            await ctx.CreateDeferred(true);
            DiscordWebhookBuilder builder = await _LookupInteractions.OutfitByTag(tag);
            await ctx.EditResponseAsync(builder);
        }

    }

    public class LookupButtonCommands : ButtonCommandModule {

        /// <summary>
        ///     Button to copy the embed of a message and print it in the channel
        /// </summary>
        public static DiscordButtonComponent PRINT_EMBED() => new(DSharpPlus.ButtonStyle.Success, $"@print-embed", "Print");

        /// <summary>
        ///     Button to show the outfit of a character
        /// </summary>
        /// <param name="charID">ID of the character to show the outfit of</param>
        /// <param name="title">Title to show on the button. Defaults to 'Outfit'</param>
        public static DiscordButtonComponent SHOW_OUTFIT(string charID, string title = "Outfit") => new(DSharpPlus.ButtonStyle.Primary, $"@show-outfit.{charID}", title);

        /// <summary>
        ///     Button to show a character
        /// </summary>
        /// <param name="charID">ID of the character</param>
        /// <param name="title">Title to show on the button. Defaults to 'Character'</param>
        public static DiscordButtonComponent SHOW_CHARACTER(string charID, string title = "Character") => new(DSharpPlus.ButtonStyle.Primary, $"@show-char.{charID}", title);

        /// <summary>
        ///     Button link to a character
        /// </summary>
        public static DiscordLinkButtonComponent LINK_CHARACTER(string charID, string title = "Honu") => new($"https://wt.honu.pw/c/{charID}", title);

        /// <summary>
        ///     Button link to an outfit
        /// </summary>
        public static DiscordLinkButtonComponent LINK_OUTFIT(string outfitID, string title = "Honu") => new($"https://wt.honu.pw/o/{outfitID}", title);

        public ILogger<LookupButtonCommands> _Logger { set; private get; } = default!;
        public LookupDiscordInteractions _LookupInteractions { set; private get; } = default!;

        /// <summary>
        ///     Button command to take the embed of a message and print it. Useful for ephemeral messages
        /// </summary>
        /// <param name="ctx">provided context</param>
        [ButtonCommand("print-embed")]
        public async Task LinkCharacterButton(ButtonContext ctx) {
            await ctx.Interaction.CreateDeferred(false);

            DiscordMessage? msg = ctx.Message;
            if (msg == null) {
                _Logger.LogDebug($"msg is null when linking character");
                await ctx.Interaction.EditResponseText("no msg");
                return;
            }

            await ctx.Interaction.EditResponseEmbed(msg.Embeds);
        }

        /// <summary>
        ///     Button command to show the information of an outfit
        /// </summary>
        /// <param name="ctx">provided context</param>
        /// <param name="charID">ID of the character to do the outfit lookup on</param>
        [ButtonCommand("show-outfit")]
        public async Task ShowOutfitInformation(ButtonContext ctx, string charID) {
            await ctx.Interaction.CreateDeferred(true);
            DiscordWebhookBuilder interactionBuilder = await _LookupInteractions.OutfitByCharacterID(charID);
            await ctx.Interaction.EditOriginalResponseAsync(interactionBuilder);
        }

        /// <summary>
        ///     Button command to show the information of a character
        /// </summary>
        /// <param name="ctx">provided context</param>
        /// <param name="charID">ID of the character</param>
        [ButtonCommand("show-char")]
        public async Task ShowCharacterInformation(ButtonContext ctx, string charID) {
            await ctx.Interaction.CreateDeferred(true);
            DiscordWebhookBuilder builder = await _LookupInteractions.CharacterLookupByID(charID);
            await ctx.Interaction.EditOriginalResponseAsync(builder);
        }

    }

    /// <summary>
    ///     Backing class for creating response to Discord interactions related to lookups
    /// </summary>
    public class LookupDiscordInteractions {

        private readonly ILogger<LookupDiscordInteractions> _Logger;
        private readonly CharacterRepository _CharacterRepository;
        private readonly OutfitRepository _OutfitRepository;
        private readonly CharacterHistoryStatRepository _CharacterHistoryStatRepository;

        public LookupDiscordInteractions(ILogger<LookupDiscordInteractions> logger,
            CharacterRepository characterRepository, OutfitRepository outfitRepository,
            CharacterHistoryStatRepository characterHistoryStatRepository) {

            _Logger = logger;

            _CharacterRepository = characterRepository;
            _OutfitRepository = outfitRepository;
            _CharacterHistoryStatRepository = characterHistoryStatRepository;
        }

        /// <summary>
        ///     Create a response for a character lookup. Can be used for both commands and buttons
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public async Task<DiscordWebhookBuilder> CharacterLookupByName(string name) {
            PsCharacter? c = await _CharacterRepository.GetFirstByName(name);

            DiscordWebhookBuilder interactionBuilder = new();
            DiscordEmbedBuilder builder = new();

            if (c == null) {
                builder.Title = $"Not found";
                builder.Description = $"Character with name `{name}` not found!";
                builder.Color = DiscordColor.Red;
                interactionBuilder.AddEmbed(builder);
                return interactionBuilder;
            }

            return await CreateCharacterResponse(c);
        }

        public async Task<DiscordWebhookBuilder> CharacterLookupByID(string charID) {
            PsCharacter? c = await _CharacterRepository.GetByID(charID, CensusEnvironment.PC, true);

            DiscordWebhookBuilder interactionBuilder = new();
            DiscordEmbedBuilder builder = new();

            if (c == null) {
                builder.Title = $"Not found";
                builder.Description = $"Character with ID `{charID}` not found!";
                builder.Color = DiscordColor.Red;
                interactionBuilder.AddEmbed(builder);
                return interactionBuilder;
            }

            return await CreateCharacterResponse(c);
        }

        /// <summary>
        ///     Create the response for a character
        /// </summary>
        /// <param name="c">Character to create the response for</param>
        private async Task<DiscordWebhookBuilder> CreateCharacterResponse(PsCharacter c) {
            DiscordEmbedBuilder builder = new();

            TrackedPlayer? player = CharacterStore.Get().GetByCharacterID(c.ID);

            List<PsCharacterHistoryStat> stats = await _CharacterHistoryStatRepository.GetByCharacterID(c.ID);

            PsCharacterHistoryStat? killsStat = stats.FirstOrDefault(iter => iter.Type == "kills");
            PsCharacterHistoryStat? deathsStat = stats.FirstOrDefault(iter => iter.Type == "deaths");
            PsCharacterHistoryStat? scoreStat = stats.FirstOrDefault(iter => iter.Type == "score");
            PsCharacterHistoryStat? timeStat = stats.FirstOrDefault(iter => iter.Type == "time");

            builder.Title = $"Character: {c.GetDisplayName()}";
            builder.Url = $"https://wt.honu.pw/c/{c.ID}";

            if (c.OutfitID != null) {
                builder.AddField("Outfit", $"[{c.OutfitTag}] {c.OutfitName}");
            } else {
                builder.AddField("Outfit", "<none>");
            }

            if (player != null) {
                builder.AddField("Online", (player.Online == true) ? $"Yes (seen on {Zone.GetName(player.ZoneID)} last)" : "No", true);
                builder.Color = (player.Online == true) ? DiscordColor.Green : DiscordColor.Purple;
            } else {
                builder.AddField("Online", "No", true);
                builder.Color = DiscordColor.Purple;
            }

            builder.AddField($"Server", World.GetName(c.WorldID), true);
            builder.AddField("Faction", Faction.GetName(c.FactionID), true);

            builder.AddField("Battle rank", $"{c.BattleRank}~{c.Prestige}", true);
            builder.AddField("Last login", c.DateLastLogin.GetDiscordRelativeTimestamp(), true);

            if (timeStat != null) {
                builder.AddField("Playtime", TimeSpan.FromSeconds(timeStat.AllTime).GetRelativeFormat(), true);
            } else {
                builder.AddField($"Playtime", "unknown", true);
            }

            if (killsStat != null && deathsStat != null) {
                builder.AddField("K/D", $"{(killsStat.AllTime / Math.Max(0d, deathsStat.AllTime)):n2}", true);
            }

            if (killsStat != null && timeStat != null) {
                builder.AddField("KPM", $"{(killsStat.AllTime / Math.Max(0d, timeStat.AllTime) * 60d):n2}", true);
            }

            if (scoreStat != null && timeStat != null) {
                builder.AddField("SPM", $"{(scoreStat.AllTime / Math.Max(0d, timeStat.AllTime) * 60d):n2}", true);
            }

            DiscordWebhookBuilder interactionBuilder = new();
            interactionBuilder.AddEmbed(builder);

            List<DiscordComponent> comps = new();
            comps.Add(LookupButtonCommands.PRINT_EMBED());
            if (c != null) {
                if (c.OutfitID != null) {
                    comps.Add(LookupButtonCommands.SHOW_OUTFIT(c.ID));
                }
                comps.Add(LookupButtonCommands.LINK_CHARACTER(c.ID));
            }

            interactionBuilder.AddComponents(comps);

            return interactionBuilder;
        }

        /// <summary>
        ///     Create a response to an outfit lookup by character ID
        /// </summary>
        /// <param name="charID"></param>
        /// <returns></returns>
        public async Task<DiscordWebhookBuilder> OutfitByCharacterID(string charID) {
            DiscordWebhookBuilder interactionBuilder = new();

            PsCharacter? c = await _CharacterRepository.GetByID(charID, CensusEnvironment.PC, true);
            if (c == null) {
                interactionBuilder.AddEmbed(new DiscordEmbedBuilder()
                    .WithTitle("Error")
                    .WithDescription($"Failed to find character ID {charID}")
                    .WithColor(DiscordColor.Red)
                );
                return interactionBuilder;
            }


            if (c.OutfitID == null) {
                interactionBuilder.AddEmbed(new DiscordEmbedBuilder()
                    .WithTitle("Error")
                    .WithDescription($"{c.GetDisplayName} is not in an outfit")
                    .WithColor(DiscordColor.Yellow)
                );
                return interactionBuilder;
            }

            PsOutfit? o = await _OutfitRepository.GetByID(c.OutfitID);
            if (o == null) {
                interactionBuilder.AddEmbed(new DiscordEmbedBuilder()
                    .WithTitle("Error")
                    .WithDescription($"Failed to find outfit with ID {c.OutfitID}")
                    .WithColor(DiscordColor.Red)
                );
                return interactionBuilder;
            }

            return await CreateOutfitEmbed(o);
        }

        /// <summary>
        ///     Create an outfit response based on tag
        /// </summary>
        /// <param name="tag">outfit tag</param>
        public async Task<DiscordWebhookBuilder> OutfitByTag(string tag) {
            DiscordWebhookBuilder builder = new();
            List<PsOutfit> outfits = await _OutfitRepository.GetByTag(tag);

            if (outfits.Count > 1) {
                builder.AddEmbed(new DiscordEmbedBuilder()
                    .WithTitle("Error")
                    .WithDescription($"There are {outfits.Count} with this tag")
                    .WithColor(DiscordColor.Yellow)
                );
                return builder;
            }

            return await CreateOutfitEmbed(outfits[0]);
        }

        /// <summary>
        ///     Create the response for an outfit
        /// </summary>
        /// <param name="outfit">Outfit to create the response for</param>
        private async Task<DiscordWebhookBuilder> CreateOutfitEmbed(PsOutfit outfit) {
            DiscordWebhookBuilder interactionBuilder = new();
            DiscordEmbedBuilder builder = new();
            List<DiscordComponent> comps = new();
            comps.Add(LookupButtonCommands.PRINT_EMBED());

            builder.Title = $"Outfit: {outfit.Name}";

            builder.Url = $"https://wt.honu.pw/o/{outfit.ID}";
            if (outfit.Tag != null) {
                builder.AddField("Tag", outfit.Tag, true);
            }

            builder.AddField("Name", outfit.Name, true);
            builder.AddField("Faction", Faction.GetName(outfit.FactionID), true);
            builder.AddField("Members", $"{outfit.MemberCount}", true);

            PsCharacter? leader = await _CharacterRepository.GetByID(outfit.LeaderID, CensusEnvironment.PC, true);
            if (leader == null) {
                builder.AddField($"Leader", $"failed to find character ID {outfit.LeaderID}!", true);
                builder.AddField("Server", "unknown (missing leader)", true); // server id for an outfit is really just the world_id of the leader
            } else {
                builder.AddField($"Leader", leader.GetDisplayName(), true);
                builder.AddField("Server", World.GetName(leader.WorldID), true);
                comps.Add(LookupButtonCommands.SHOW_CHARACTER(leader.ID, "Leader"));
                comps.Add(LookupButtonCommands.LINK_CHARACTER(leader.ID, "Leader"));
            }

            comps.Add(LookupButtonCommands.LINK_OUTFIT(outfit.ID));

            interactionBuilder.AddComponents(comps);
            interactionBuilder.AddEmbed(builder);

            return interactionBuilder;
        }

    }

}
