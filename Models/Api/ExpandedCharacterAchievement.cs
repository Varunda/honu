using watchtower.Models.Census;
using watchtower.Services.Census;

namespace watchtower.Models.Api {

    public class ExpandedCharacterAchievement {

        public CharacterAchievement Entry { get; set; } = new();

        /// <summary>
        ///     corresponds to the <see cref="CharacterAchievement.AchievementID"/>
        /// </summary>
        public Achievement? Achievement { get; set; } = null;

        public PsObjective? Objective { get; set; } = null;

        /// <summary>
        ///     corresponds to the <see cref="Achievement.ItemID"/>
        /// </summary>
        public PsItem? Item { get; set; } = null;

    }
}
