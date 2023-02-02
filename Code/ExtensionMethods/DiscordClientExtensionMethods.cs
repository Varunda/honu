using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using System.Threading.Tasks;

namespace watchtower.Code.ExtensionMethods {

    public static class DiscordClientExtensionMethods {

        /// <summary>
        ///     Try to get a guild, catching a NotFoundException and instead returning null
        /// </summary>
        /// <param name="client">Extension instance</param>
        /// <param name="guildID">ID of the guild to get</param>
        /// <returns>
        ///     The <see cref="DiscordGuild"/> with <see cref="DiscordGuild.Id"/> of <paramref name="guildID"/>,
        ///     or <c>null</c> if not found
        /// </returns>
        public static async Task<DiscordGuild?> TryGetGuild(this DiscordClient client, ulong guildID) {
            try {
                return await client.GetGuildAsync(guildID);
            } catch (NotFoundException) {
                return null;
            }
        }

        /// <summary>
        ///     Try to get a <see cref="DiscordMember"/> from a <see cref="DiscordGuild"/>,
        ///     catching a <see cref="NotFoundException"/> and returning null instead
        /// </summary>
        /// <param name="guild">Extension instance</param>
        /// <param name="memberID">ID of the member to get</param>
        /// <returns>
        ///     The <see cref="DiscordMember"/> with <see cref="SnowflakeObject.Id"/> of <paramref name="memberID"/>,
        ///     or <c>null</c> if the member could not be found
        /// </returns>
        public static async Task<DiscordMember?> TryGetMember(this DiscordGuild guild, ulong memberID) {
            try {
                return await guild.GetMemberAsync(memberID);
            } catch (NotFoundException) {
                return null;
            }
        }

    }
}
