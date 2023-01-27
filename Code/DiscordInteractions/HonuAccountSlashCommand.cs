using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models;
using watchtower.Services;
using watchtower.Services.Db;
using watchtower.Services.Repositories;

namespace watchtower.Code.DiscordInteractions {

    public class HonuAccountSlashCommand : PermissionSlashCommand {

        public ILogger<HonuAccountSlashCommand> _Logger { set; private get; } = default!;
        public HonuAccountDbStore _AccountDb { set; private get; } = default!;

        /// <summary>
        ///     Slash command to get the current <see cref="HonuAccount"/>
        /// </summary>
        /// <param name="ctx">Provided context</param>
        [SlashCommand("whoami", "See what permissions you have in a command")]
        public async Task WhoAmICommand(InteractionContext ctx) {
            HonuAccount? account = await _CurrentUser.GetDiscord(ctx);
            if (account == null) {
                await ctx.CreateImmediateText($"You do not have a Honu account");
                return;
            }

            List<HonuAccountPermission> perms = await _PermissionRepository.GetByAccountID(account.ID);
            string s = $"Permissions on this account ({perms.Count}): \n";
            s += string.Join("\n", perms.Select(iter => $"`{iter.Permission}`"));

            await ctx.CreateImmediateText($"Honu account ID: {account.ID}\n{s}");
        }

        /// <summary>
        ///     Slash command to get the permission of a target user
        /// </summary>
        /// <param name="ctx">Provided context</param>
        /// <param name="user">Discord user to get the permissions of</param>
        [SlashCommand("whois", "See what permissions another account has")]
        public async Task WhoIsCommand(InteractionContext ctx,
            [Option("user", "What user to target with this command")] DiscordUser user) {

            HonuAccount? currentUser = await _CurrentUser.GetDiscord(ctx);
            if (currentUser == null) {
                await ctx.CreateImmediateText($"You do not have a Honu account, not executing command");
                return;
            }

            HonuAccount? targetAccount = await _AccountDb.GetByDiscordID(user.Id, CancellationToken.None);
            if (targetAccount == null) {
                await ctx.CreateImmediateText($"Target user does not have a Honu account");
                return;
            }

            List<HonuAccountPermission> perms = await _PermissionRepository.GetByAccountID(targetAccount.ID);
            string s = $"Permissions on this account ({perms.Count}): \n";
            s += string.Join("\n", perms.Select(iter => $"`{iter.Permission}"));

            await ctx.CreateImmediateText($"Honu account ID: {targetAccount.ID}\n{s}");
        }

        [ContextMenu(ApplicationCommandType.UserContextMenu, "Honu account whois")]
        public async Task WhoIsContext(ContextMenuContext ctx) {
            DiscordMember source = ctx.Member;
            DiscordMember target = ctx.TargetMember;

            HonuAccount? currentUser = await _CurrentUser.GetDiscord(ctx);
            if (currentUser == null) {
                await ctx.CreateImmediateText($"You do not have a Honu account, not executing interaction");
                return;
            }

            HonuAccount? targetAccount = await _AccountDb.GetByDiscordID(target.Id, CancellationToken.None);
            if (targetAccount == null) {
                await ctx.CreateImmediateText($"Target user does not have a Honu account");
                return;
            }

            List<HonuAccountPermission> perms = await _PermissionRepository.GetByAccountID(targetAccount.ID);
            string s = $"Permissions on this account ({perms.Count}): \n";
            s += string.Join("\n", perms.Select(iter => $"`{iter.Permission}`"));

            await source.SendMessageAsync($"Honu account ID: {targetAccount.ID}\n{s}");

            await ctx.CreateImmediateText($"Sent information in a DM!");
        }

    }
}
