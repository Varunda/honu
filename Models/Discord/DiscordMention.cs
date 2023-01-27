namespace watchtower.Models.Discord {

    public class DiscordMention {

        public enum DiscordMentionType {
            NONE,
            USER,
            ROLE,
            CHANNEL
        }

        public DiscordMentionType MentionType { get; set; } = DiscordMentionType.NONE;
        
    }

    public class RoleDiscordMention : DiscordMention {

        public ulong RoleID { get; }

        public RoleDiscordMention(ulong roleID) {
            RoleID = roleID;
            MentionType = DiscordMentionType.ROLE;
        }

    }

}
