using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Discord;
using watchtower.Models.Internal;
using watchtower.Models.PSB;
using watchtower.Services.Queues;

namespace watchtower.Code.DiscordInteractions {

    public class PingSlashCommand : PermissionSlashCommand {

        public ILogger<PingSlashCommand> _Logger { set; private get; } = default!;
        public DiscordMessageQueue _Discord { set; private get; } = default!;
        public IOptions<JaegerNsaOptions> _Options { set; private get; } = default!;

        [SlashCommand("ping", "Ping Honu/Spark33")]
        public async Task PingCommand(InteractionContext ctx) {
            DiscordInteractionResponseBuilder response = new DiscordInteractionResponseBuilder().WithContent("pong");

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, response);

            _Logger.LogDebug($"ping!");
        }

        [SlashCommand("at-me", "Test command to ensure role pings work")]
        public async Task AtCommand(InteractionContext ctx) {
            if (await _CheckPermission(ctx, HonuPermission.HONU_DISCORD_ADMIN) == false) {
                return;
            }

            Models.Discord.DiscordMessage msg = new();
            msg.Message = $"ping! <@&{_Options.Value.AlertRoleID}>";

            DiscordMention mention = new RoleDiscordMention((ulong)(_Options.Value.AlertRoleID ?? 0));
            msg.Mentions.Add(mention);

            _Discord.Queue(msg);

            await ctx.CreateImmediateText($"Pinged!");
        }

    }
}
