using DaybreakGames.Census;
using watchtower.Models.Census;

namespace watchtower.Services.Census {

    public class ObjectiveSetCollection : BaseStaticCollection<ObjectiveSet> {

        public ObjectiveSetCollection(ICensusQueryFactory census, ICensusReader<ObjectiveSet> reader)
            : base("objective_set_to_objective", census, reader) { }

    }

}