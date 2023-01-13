using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace watchtower.Code.SlashCommands {

    public class PingSlashCommand : ApplicationCommandModule {

        public ILogger<PingSlashCommand> _Logger { set; private get; } = default!;

        [SlashCommand("ping", "Ping Honu/Spark33")]
        public async Task PingCommand(InteractionContext ctx) {
            DiscordInteractionResponseBuilder response = new DiscordInteractionResponseBuilder().WithContent("pong");

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, response);

            _Logger.LogDebug($"ping!");
        }

    }
}
