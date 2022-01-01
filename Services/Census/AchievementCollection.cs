using DaybreakGames.Census;
using watchtower.Models.Census;

namespace watchtower.Services.Census {

    /// <summary>
    ///     Achievement collection
    /// </summary>
    public class AchievementCollection : BaseStaticCollection<Achievement> {

        public AchievementCollection(ICensusQueryFactory census, ICensusReader<Achievement> reader)
            : base("achievement", census, reader) {
        }

    }
}
