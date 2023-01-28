using DSharpPlus.SlashCommands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models;
using watchtower.Services.Db;

namespace watchtower.Code.DiscordInteractions {

    /// <summary>
    ///     Attribute to require a user to have a Honu permission
    /// </summary>
    public class RequiredHonuPermissionAttribute {

        public static async Task<bool> Execute(BaseContext ctx, List<string> permissions) {
            ILogger<RequiredHonuPermissionAttribute> logger = ctx.Services.GetRequiredService<ILogger<RequiredHonuPermissionAttribute>>();
            HonuAccountDbStore accountDb = ctx.Services.GetRequiredService<HonuAccountDbStore>();

            HonuAccount? account = await accountDb.GetByDiscordID(ctx.User.Id, CancellationToken.None);
            if (account == null) {
                logger.LogTrace($"User {ctx.User.Id} does not have a Honu account");
                return false;
            }

            HonuAccountPermissionDbStore permDb = ctx.Services.GetRequiredService<HonuAccountPermissionDbStore>();

            // get at least one HonuAccountPermssion the user has
            List<HonuAccountPermission> perms = await permDb.GetByAccountID(account.ID);
            HonuAccountPermission? perm = perms.FirstOrDefault(iter => permissions.IndexOf(iter.Permission) > -1);

            if (perm == null) {
                logger.LogDebug($"User {ctx.User.GetDisplay()} lacks any of the following permissions: {string.Join(", ", permissions)}");
                return false;
            }

            logger.LogDebug($"User {ctx.User.GetDisplay()} has permission {perm.ID}/{perm.Permission}");
            return true;
        }

    }

    /// <summary>
    ///     Attribute on slash commands to require a Honu permission
    /// </summary>
    public class RequiredHonuPermissionSlashAttribute : SlashCheckBaseAttribute {

        public List<string> Permissions { get; }

        public RequiredHonuPermissionSlashAttribute(params string[] perms) {
            Permissions = perms.ToList();
        }

        public override Task<bool> ExecuteChecksAsync(InteractionContext ctx) {
            return RequiredHonuPermissionAttribute.Execute(ctx, Permissions);
        }
    }

    /// <summary>
    ///     Attribute on a context menu command to require a Honu permission
    /// </summary>
    public class RequiredHonuPermissionContextAttribute : ContextMenuCheckBaseAttribute {

        public List<string> Permissions { get; }

        public RequiredHonuPermissionContextAttribute(params string[] perms) {
            Permissions = perms.ToList();
        }

        public override Task<bool> ExecuteChecksAsync(ContextMenuContext ctx) {
            return RequiredHonuPermissionAttribute.Execute(ctx, Permissions);
        }

    }

}
