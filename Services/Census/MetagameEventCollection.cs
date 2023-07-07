using DaybreakGames.Census;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Models.Census;

namespace watchtower.Services.Census {

    public class MetagameEventCollection : BaseStaticCollection<PsMetagameEvent> {

        public MetagameEventCollection(ILogger<BaseStaticCollection<PsMetagameEvent>> logger,
            ICensusQueryFactory census, ICensusReader<PsMetagameEvent> reader)

            : base(logger, "metagame_event", census, reader) {
        }

    }
}
