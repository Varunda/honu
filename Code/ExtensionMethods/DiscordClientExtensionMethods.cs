using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using System.Threading.Tasks;

namespace watchtower.Code.ExtensionMethods {

    public static class DiscordClientExtensionMethods {

        public static async Task<DiscordGuild?> TryGetGuild(this DiscordClient client, ulong guildID) {
            try {
                return await client.GetGuildAsync(guildID);
            } catch (NotFoundException) {
                return null;
            }
        }

        public static async Task<DiscordMember?> TryGetMember(this DiscordGuild guild, ulong memberID) {
            try {
                return await guild.GetMemberAsync(memberID);
            } catch (NotFoundException) {
                return null;
            }
        }

    }
}
