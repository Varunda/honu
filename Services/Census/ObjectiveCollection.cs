
using DaybreakGames.Census;
using Microsoft.Extensions.Logging;
using watchtower.Models.Census;

namespace watchtower.Services.Census {

    public class ObjectiveCollection : BaseStaticCollection<PsObjective> {

        public ObjectiveCollection(ILogger<ObjectiveCollection> logger,
            ICensusQueryFactory census, ICensusReader<PsObjective> reader, ILoggerFactory fac)
            : base(logger, "objective", census, reader) { }

    }

}