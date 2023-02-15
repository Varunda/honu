using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;

namespace watchtower.Code.DiscordInteractions {

    public class Ps2ServerCommands : PermissionSlashCommand {

        public ILogger<Ps2ServerCommands> _Logger { set; private get; } = default!;

        public enum StatusWorlds : short {
            [ChoiceName("Connery")] Connery = 1,
            [ChoiceName("Emerald")] Emerald = 17,
            [ChoiceName("Cobalt")] Cobalt = 10,
            [ChoiceName("Miller")] Miller = 13,
            [ChoiceName("SolTech")] SolTech = 40,
            [ChoiceName("Jaeger")] Jaeger = 19
        }

        //[SlashCommand("status", "Look up the status of a server")]
        public async Task ServerStatusCommand(InteractionContext ctx,
            [Option("server", "Which server")] StatusWorlds world) {

            await ctx.CreateDeferred(true);

            short worldID = (short)world;
            _Logger.LogDebug($"status for {worldID}");

            await ctx.EditResponseText("done!");
        }

    }
}
