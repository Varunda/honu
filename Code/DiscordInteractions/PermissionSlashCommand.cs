using DSharpPlus.SlashCommands;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models;
using watchtower.Services;
using watchtower.Services.Repositories;
using watchtower.Services.Repositories.PSB;

namespace watchtower.Code.DiscordInteractions {

    /// <summary>
    ///     Class to inherit from if executing a Discord slash command requires a specific Honu account permission
    /// </summary>
    public class PermissionSlashCommand : ApplicationCommandModule {

        public PsbDriveRepository _PsbDrive { internal get; set; } = default!;
        public CurrentHonuAccount _CurrentUser { internal get; set; } = default!;
        public HonuAccountPermissionRepository _PermissionRepository { internal get; set; } = default!;

        /// <summary>
        ///     Check if a Discord user performing a slash command has the correct Honu account permission
        /// </summary>
        /// <param name="ctx">InteractionContext from the method</param>
        /// <param name="permissions">List of permissions that user can have. This is an OR operation</param>
        /// <returns>
        ///     True if the user performing the command in <paramref name="ctx"/>
        ///     has one of the Honu permissions passed in <paramref name="permissions"/>,
        ///     otherwise <c>false</c>, which will also respond to the command with an appropriate message
        /// </returns>
        internal async Task<bool> _CheckPermission(InteractionContext ctx, params string[] permissions) {
            HonuAccount? user = await _CurrentUser.GetDiscord(ctx);
            if (user == null) {
                await ctx.CreateImmediateText("You do not have a Honu account");
                return false;
            }

            HonuAccountPermission? neededPerm = await _PermissionRepository.GetPermissionByAccount(user, permissions);
            if (neededPerm == null) {
                await ctx.CreateImmediateText("You lack the correct permission");
                return false;
            }

            return true;
        }

    }
}
