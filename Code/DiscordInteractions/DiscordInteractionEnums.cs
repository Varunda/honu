using DSharpPlus.SlashCommands;
using watchtower.Code.Constants;

namespace watchtower.Code.DiscordInteractions {

    public class DiscordInteractionEnums {

        /// <summary>
        ///     Contains the worlds
        /// </summary>
        public enum StatusWorlds : int {
            [ChoiceName("Connery")] Connery = World.Connery,
            [ChoiceName("Emerald")] Emerald = World.Emerald,
            [ChoiceName("Cobalt")] Cobalt = World.Cobalt,
            [ChoiceName("Miller")] Miller = World.Miller,
            [ChoiceName("SolTech")] SolTech = World.SolTech,
            [ChoiceName("Jaeger")] Jaeger = World.Jaeger
        }

    }
}
