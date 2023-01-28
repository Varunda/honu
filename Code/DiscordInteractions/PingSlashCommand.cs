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

        [SlashCommand("ping", "Ping Honu/Spark33")]
        public async Task PingCommand(InteractionContext ctx) {
            await ctx.CreateImmediateText($"pong", true);
            _Logger.LogDebug($"ping!");
        }

    }
}
