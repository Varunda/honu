using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Internal;

namespace watchtower.Code.SlashCommands {

    [SlashCommandGroup("internal", "Internal commands")]
    public class HonuInternalSlashCommands : PermissionSlashCommand {

        public ILogger<HonuInternalSlashCommands> _Logger { private get; set; } = default!;

        /// <summary>
        ///     Slash command to refresh commands
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        [SlashCommand("reload-commands", "Reload commands")]
        public async Task ReloadCommands(InteractionContext ctx) {
            if (await _CheckPermission(ctx, HonuPermission.HONU_DISCORD_ADMIN, HonuPermission.HONU_ACCOUNT_ADMIN) == false) {
                return;
            }

            int commandCount = ctx.Client.GetSlashCommands().RegisteredCommands.Count;
            await ctx.CreateDeferredText($"Reloading {commandCount} commands...");

            await ctx.Client.GetSlashCommands().RefreshCommands();

            await ctx.EditResponseText("Commands reloaded!");
        }

    }
}
