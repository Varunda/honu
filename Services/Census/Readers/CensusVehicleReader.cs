using Newtonsoft.Json.Linq;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Census;

namespace watchtower.Services.Census.Readers {

    public class CensusVehicleReader : ICensusReader<PsVehicle> {

        public override PsVehicle? ReadEntry(JToken token) {
            PsVehicle veh = new PsVehicle();

            veh.ID = token.GetRequiredInt32("vehicle_id");
            veh.Name = token.SelectToken("name")?.GetString("en", "<missing en name>") ?? "<missing name>";
            veh.Description = token.SelectToken("description")?.GetString("en", "<missing en description>") ?? "<missing description>";
            veh.TypeID = token.GetInt32("type_id", -1);
            veh.CostResourceID = token.GetInt32("cost_resource_id", -1);
            veh.ImageSetID = token.GetInt32("image_set_id", 0);
            veh.ImageID = token.GetInt32("image_id", 0);

            return veh;
        }

    }
}
