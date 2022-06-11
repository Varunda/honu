using DaybreakGames.Census;
using Microsoft.Extensions.Logging;
using watchtower.Models.Census;

namespace watchtower.Services.Census {

    public class VehicleCollection : BaseStaticCollection<PsVehicle> {

        public VehicleCollection(ILogger<VehicleCollection> logger,
            ICensusQueryFactory census, ICensusReader<PsVehicle> reader, ILoggerFactory fac)
            : base(logger, "vehicle", census, reader) {
        }

    }
}
