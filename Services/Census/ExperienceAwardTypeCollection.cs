using DaybreakGames.Census;
using Microsoft.Extensions.Logging;
using watchtower.Models.Census;

namespace watchtower.Services.Census {

    public class ExperienceAwardTypeCollection : BaseStaticCollection<ExperienceAwardType> {

        public ExperienceAwardTypeCollection(ILogger<BaseStaticCollection<ExperienceAwardType>> logger,
                ICensusQueryFactory census, ICensusReader<ExperienceAwardType> reader)
            : base(logger, "experience_award_type", census, reader) {
        }

    }
}
