using DaybreakGames.Census;
using Microsoft.Extensions.Logging;
using watchtower.Models.Census;

namespace watchtower.Services.Census {

    public class ObjectiveSetCollection : BaseStaticCollection<ObjectiveSet> {

        public ObjectiveSetCollection(ILogger<ObjectiveSetCollection> logger, 
            ICensusQueryFactory census, ICensusReader<ObjectiveSet> reader, ILoggerFactory fac)
            : base(logger, "objective_set_to_objective", census, reader) { }

    }

}