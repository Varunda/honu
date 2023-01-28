using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.PSB;

namespace watchtower.Code.DiscordInteractions {

    public static class RequiredRoleCheck {

        public const string OVO_STAFF = "ovo-staff";

        public const string OVO_ADMIN = "ovo-admin";

        public const string PRACTICE_STAFF = "practice-staff";

        public const string PRACTICE_ADMIN = "practice-admin";

        public static Task<bool> Execute(BaseContext ctx, List<string> roles) {
            IOptions<PsbRoleMapping> mapping = ctx.Services.GetRequiredService<IOptions<PsbRoleMapping>>();
            ILogger<RequiredRoleSlashAttribute> logger = ctx.Services.GetRequiredService<ILogger<RequiredRoleSlashAttribute>>();

            List<ulong> allowedRoles = new();

            foreach (string role in roles) {
                if (mapping.Value.Mappings.TryGetValue(role, out ulong roleId) == true) {
                    allowedRoles.Add(roleId);
                    logger.LogTrace($"Role mapping '{role}' maps to {roleId}");
                } else {
                    throw new SystemException($"failed to find role {role}: no mapping exists");
                }
            }

            if (ctx.Member == null) {
                return Task.FromResult(false);
            }

            DiscordRole? permittedRole = ctx.Member.Roles.FirstOrDefault(iter => allowedRoles.IndexOf(iter.Id) > -1);
            logger.LogTrace($"Role {permittedRole?.Id}/{permittedRole?.Name} found for interaction {ctx.CommandName} {ctx.User.GetDisplay()}");

            return Task.FromResult(ctx.Member.Roles.FirstOrDefault(iter => allowedRoles.IndexOf(iter.Id) > -1) != null);
        }
    }

    public class RequiredRoleSlashAttribute : SlashCheckBaseAttribute {

        public List<string> Roles { get; } = new();

        public RequiredRoleSlashAttribute(params string[] roles) {
            this.Roles = roles.ToList();
        }

        public override Task<bool> ExecuteChecksAsync(InteractionContext ctx) {
            return RequiredRoleCheck.Execute(ctx, Roles);
        }

    }

    public class RequiredRoleContextAttribute : ContextMenuCheckBaseAttribute {

        public List<string> Roles { get; } = new();

        public RequiredRoleContextAttribute(params string[] roles) {
            this.Roles = roles.ToList();
        }

        public override Task<bool> ExecuteChecksAsync(ContextMenuContext ctx) {
            return RequiredRoleCheck.Execute(ctx, Roles);
        }
    }

}
