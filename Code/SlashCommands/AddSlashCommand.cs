using DSharpPlus;
using DSharpPlus.SlashCommands;
using System.Threading.Tasks;

namespace watchtower.Code.SlashCommands {

    public class AddSlashCommand : ApplicationCommandModule {

        [SlashCommand("add", "yeah hahah that's funny")]
        public async Task AddCommand(InteractionContext ctx) {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DSharpPlus.Entities.DiscordInteractionResponseBuilder().WithContent("howdy"));
        }

    }
}
