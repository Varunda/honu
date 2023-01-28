using DSharpPlus.Entities;

namespace watchtower.Code.ExtensionMethods {

    public static class DiscordUserExtentionMethods {

        public static string GetPing(this DiscordUser user) {
            return $"<@{user.Id}>";
        }

        public static string GetDisplay(this DiscordUser user) {
            return $"{user.Username}#{user.Discriminator} ({user.Id})";
        }

    }
}
