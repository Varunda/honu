using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Discord;
using watchtower.Models.Internal;
using watchtower.Models.PSB;
using watchtower.Services.Queues;

namespace watchtower.Code.DiscordInteractions {

    [SlashCommandGroup("internal", "Internal commands")]
    public class HonuInternalSlashCommands : PermissionSlashCommand {

        public ILogger<HonuInternalSlashCommands> _Logger { private get; set; } = default!;

        public IOptions<JaegerNsaOptions> _NsaOptions { set; private get; } = default!;
        public IOptions<PsbRoleMapping> _RoleMappings { set; private get; } = default!;

        public DiscordMessageQueue _Discord { set; private get; } = default!;

        /// <summary>
        ///     Slash command to refresh commands
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        [SlashCommand("reload-commands", "Reload commands")]
        [RequiredHonuPermissionSlash(HonuPermission.HONU_DISCORD_ADMIN)]
        public async Task ReloadCommands(InteractionContext ctx) {
            int commandCount = ctx.Client.GetSlashCommands().RegisteredCommands.Count;
            await ctx.CreateDeferredText($"Reloading {commandCount} commands...", true);

            await ctx.Client.GetSlashCommands().RefreshCommands();

            await ctx.EditResponseText("Commands reloaded!");
        }

        /// <summary>
        ///     Internal command to ensure role pings work
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        [SlashCommand("ping-flippers", "Test command to ensure role pings work")]
        [RequiredHonuPermissionContext(HonuPermission.HONU_DISCORD_ADMIN)]
        public async Task PingFlippersCommand(InteractionContext ctx) {
            HonuDiscordMessage msg = new();
            msg.Message = $"ping! <@&{_NsaOptions.Value.AlertRoleID}>";
            msg.ChannelID = _NsaOptions.Value.ChannelID;
            msg.GuildID = _NsaOptions.Value.GuildID;

            RoleMention mention = new RoleMention(_NsaOptions.Value.AlertRoleID ?? 0);
            msg.Mentions.Add(mention);

            _Discord.Queue(msg);

            await ctx.CreateImmediateText($"Pinged!");
        }

        /// <summary>
        ///     Slash command to print out the role mappings used for <see cref="RequiredRoleCheck"/>
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        [SlashCommand("debug-role-mappings", "Pring role mappings")]
        [RequiredHonuPermissionSlash(HonuPermission.HONU_DISCORD_ADMIN)]
        public async Task DebugRoleMappings(InteractionContext ctx) {
            DiscordInteractionResponseBuilder builder = new();
            builder.AsEphemeral(true);

            DiscordEmbedBuilder embed = new();
            embed.Description = $"Discord role mappings (name => role):";

            foreach (KeyValuePair<string, ulong> role in _RoleMappings.Value.Mappings) {
                embed.AddField(role.Key, $"<@&{role.Value}>", inline: true);
            }

            builder.AddEmbed(embed.Build());

            await ctx.CreateResponseAsync(builder);
        }

        [SlashCommand("test-channel-msg", "Send a test message to a channel")]
        [RequiredHonuPermissionSlash(HonuPermission.HONU_DISCORD_ADMIN)]
        public async Task DebugMessageChannel(InteractionContext ctx,
            [Option("GuildID", "guild id")] string guildID,
            [Option("ChannelID", "channel id")] string channelID) {

            HonuDiscordMessage msg = new();
            msg.ChannelID = ulong.Parse(channelID);
            msg.GuildID = ulong.Parse(guildID);
            msg.Message = $"Test message from {ctx.User.Id}";

            _Discord.Queue(msg);

            await ctx.CreateImmediateText($"sent", true);
        }

        [SlashCommand("test-user-msg", "send a test message to a user")]
        [RequiredHonuPermissionSlash(HonuPermission.HONU_DISCORD_ADMIN)]
        public async Task DebugMessageUser(InteractionContext ctx,
            [Option("Member", "Target member")] DiscordUser member) {

            HonuDiscordMessage msg = new();
            msg.TargetUserID = member.Id;

            msg.Message = $"Test message from {ctx.User.Id}";

            _Discord.Queue(msg);

            await ctx.CreateImmediateText("sent", true);
        }

    }
}
