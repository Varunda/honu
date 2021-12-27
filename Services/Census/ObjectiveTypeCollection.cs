using DaybreakGames.Census;
using watchtower.Models.Census;

namespace watchtower.Services.Census {

    public class ObjectiveTypeCollection : BaseStaticCollection<ObjectiveType> {

        public ObjectiveTypeCollection(ICensusQueryFactory census, ICensusReader<ObjectiveType> reader)
            : base("objective_type", census, reader) { }

    }

}