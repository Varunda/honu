using DaybreakGames.Census;
using Microsoft.Extensions.Logging;
using watchtower.Models.Census;

namespace watchtower.Services.Census {

    public class ObjectiveTypeCollection : BaseStaticCollection<ObjectiveType> {

        public ObjectiveTypeCollection(ILogger<ObjectiveTypeCollection> logger,
            ICensusQueryFactory census, ICensusReader<ObjectiveType> reader, ILoggerFactory fac)
            : base(logger, "objective_type", census, reader) { }

    }

}