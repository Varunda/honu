using Npgsql;
using System.Data;
using watchtower.Models.Census;

namespace watchtower.Services.Db.Readers {

    public class VehicleDataReader : IDataReader<PsVehicle> {

        public override PsVehicle? ReadEntry(NpgsqlDataReader reader) {
            PsVehicle veh = new PsVehicle();

            veh.ID = reader.GetInt32("id");
            veh.Name = reader.GetString("name");
            veh.Description = reader.GetString("description");
            veh.TypeID = reader.GetInt32("type_id");
            veh.CostResourceID = reader.GetInt32("cost_resource_id");
            veh.ImageID = reader.GetInt32("image_id");
            veh.ImageSetID = reader.GetInt32("image_set_id");

            return veh;
        }

    }

}
