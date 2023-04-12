using DaybreakGames.Census;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using watchtower.Models.Census;

namespace watchtower.Services.Census {

    public class FireGroupToFireModeCollection : BaseStaticCollection<FireGroupToFireMode> {

        public FireGroupToFireModeCollection(ILogger<BaseStaticCollection<FireGroupToFireMode>> logger,
            ICensusQueryFactory census, ICensusReader<FireGroupToFireMode> reader)
            : base(logger, "fire_group_to_fire_mode", census, reader) {
        }

    }
}
