using DaybreakGames.Census;
using Microsoft.Extensions.Logging;
using watchtower.Models.Census;

namespace watchtower.Services.Census {

    public class ExperienceTypeCollection : BaseStaticCollection<ExperienceType> {

        public ExperienceTypeCollection(ILogger<BaseStaticCollection<ExperienceType>> logger,
                ICensusQueryFactory census, ICensusReader<ExperienceType> reader)
            : base(logger, "experience", census, reader) { }

    }
}
