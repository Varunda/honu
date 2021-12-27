
using DaybreakGames.Census;
using watchtower.Models.Census;

namespace watchtower.Services.Census {

    public class ObjectiveCollection : BaseStaticCollection<PsObjective> {

        public ObjectiveCollection(ICensusQueryFactory census, ICensusReader<PsObjective> reader)
            : base("objective", census, reader) { }

    }

}