using DaybreakGames.Census;
using Microsoft.Extensions.Logging;
using watchtower.Models.Census;

namespace watchtower.Services.Census {

    /// <summary>
    ///     Achievement collection
    /// </summary>
    public class AchievementCollection : BaseStaticCollection<Achievement> {

        public AchievementCollection(ILogger<AchievementCollection> logger, 
            ICensusQueryFactory census, ICensusReader<Achievement> reader)
            : base(logger, "achievement", census, reader) {
        }

    }
}
