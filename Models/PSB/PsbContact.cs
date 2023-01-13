namespace watchtower.Models.PSB {

    public class PsbContact {

        /// <summary>
        ///     Discord ID. Is 0 if not available
        /// </summary>
        public ulong DiscordID { get; set; }

        /// <summary>
        ///     Email to reach the person by
        /// </summary>
        public string Email { get; set; } = "";

        /// <summary>
        ///     Name the person is known by
        /// </summary>
        public string Name { get; set; } = "";

    }
}
