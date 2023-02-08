using DSharpPlus.ButtonCommands;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
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

        [SlashCommand("character", "Do a character lookup")]
        public async Task CharacterLookupCommand(InteractionContext ctx,
            [Option("name", "Character name")] string name) {

            await ctx.CreateDeferred(true);

            PsCharacter? c = await _CharacterRepository.GetFirstByName(name);

            DiscordEmbedBuilder builder = new();

            if (c == null) {
                builder.Title = $"Not found";
                builder.Description = $"Character with name `{name}` not found!";
                builder.Color = DiscordColor.Red;
            } else {

                TrackedPlayer? player = CharacterStore.Get().GetByCharacterID(c.ID);

                builder.Title = $"{c.GetDisplayName()}";
                builder.Color = DiscordColor.Green;
                builder.Url = $"https://wt.honu.pw/c/{c.ID}";

                if (player != null) {
                    builder.AddField("Online", (player.Online == true) ? $"true (seen on {Zone.GetName(player.ZoneID)} last)" : "false");
                } else {
                    builder.AddField("Online", "false");
                }

                builder.AddField($"Server", World.GetName(c.WorldID));
            }

            DiscordWebhookBuilder interactionBuilder = new();
            interactionBuilder.AddEmbed(builder);
            interactionBuilder.AddComponents(LookupButtonCommands.LINK_CHARACTER("0"));

            await ctx.EditResponseAsync(interactionBuilder);
        }

    }

    public class LookupButtonCommands : ButtonCommandModule {

        /// <summary>
        ///     Button to link a character lookup in the channel the command was used in
        /// </summary>
        /// <param name="charID">ID of the character</param>
        /// <returns></returns>
        public static DiscordButtonComponent LINK_CHARACTER(string charID) => new(DSharpPlus.ButtonStyle.Primary, $"@char-link.{charID}", "Link");

        public ILogger<LookupButtonCommands> _Logger { set; private get; } = default!;

        [ButtonCommand("char-link")]
        public async Task LinkCharacterButton(ButtonContext ctx, string charID) {
            await ctx.Interaction.CreateDeferred(false);

            DiscordMessage? msg = ctx.Message;
            if (msg == null) {
                _Logger.LogDebug($"msg is null when linking character");
                await ctx.Interaction.EditResponseText("no msg");
                return;
            }

            await ctx.Interaction.EditResponseEmbed(msg.Embeds);
        }

    }

}
