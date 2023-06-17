using DSharpPlus.Entities;
using System.Linq;

namespace watchtower.Code.ExtensionMethods {

    public static class DiscordMemberExtensionMethods {

        /// <summary>
        ///     Check if a <see cref="DiscordMember"/> has one of the roles provided in <paramref name="roleIDs"/>
        /// </summary>
        /// <param name="member">extension instance</param>
        /// <param name="roleIDs">List of role IDs</param>
        /// <returns>
        ///     If <paramref name="member"/> has at least one <see cref="DiscordRole"/>
        ///     with a <see cref="SnowflakeObject.Id"/> within <paramref name="roleIDs"/>
        /// </returns>
        public static bool HasRole(this DiscordMember member, params ulong[] roleIDs) {
            foreach (ulong roleID in roleIDs) {
                if (member.Roles.FirstOrDefault(iter => iter.Id == roleID) != null) {
                    return true;
                }
            }
            return false;
        }

    }
}
