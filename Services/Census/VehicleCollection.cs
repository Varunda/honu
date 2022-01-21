using DaybreakGames.Census;
using watchtower.Models.Census;

namespace watchtower.Services.Census {

    public class VehicleCollection : BaseStaticCollection<PsVehicle> {

        public VehicleCollection(ICensusQueryFactory census, ICensusReader<PsVehicle> reader)
            : base("vehicle", census, reader) {
        }

    }
}
