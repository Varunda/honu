using Npgsql;
using watchtower.Models.Events;

namespace watchtower.Services.Db.Readers {

    public class VehicleDestroyEventReader : IDataReader<VehicleDestroyEvent> {
        public override VehicleDestroyEvent? ReadEntry(NpgsqlDataReader reader) {
            throw new System.NotImplementedException();
        }
    }
}
