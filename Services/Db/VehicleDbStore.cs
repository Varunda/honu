using Microsoft.Extensions.Logging;
using Npgsql;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Census;

namespace watchtower.Services.Db {

    public class VehicleDbStore : BaseStaticDbStore<PsVehicle> {

        public VehicleDbStore(ILoggerFactory loggerFactory,
                IDataReader<PsVehicle> reader, IDbHelper helper)
            : base("vehicle", loggerFactory, reader, helper) {
        }

        internal override void SetupUpsertCommand(NpgsqlCommand cmd, PsVehicle param) {
            cmd.CommandText = @"
                INSERT INTO vehicle (
                    id, name, description, type_id, cost_resource_id, image_set_id, image_id
                ) VALUES (
                    @ID, @Name, @Description, @TypeID, @CostResourceID, @ImageSetID, @ImageID
                ) ON CONFLICT (id) DO
                    UPDATE SET name = @Name,
                        description = @Description,
                        type_id = @TypeID,
                        cost_resource_id = @CostResourceID,
                        image_set_id = @ImageSetID,
                        image_id = @ImageID;
            ";

            cmd.AddParameter("ID", param.ID);
            cmd.AddParameter("Name", param.Name);
            cmd.AddParameter("Description", param.Description);
            cmd.AddParameter("TypeID", param.TypeID);
            cmd.AddParameter("CostResourceID", param.CostResourceID);
            cmd.AddParameter("ImageSetID", param.ImageSetID);
            cmd.AddParameter("ImageID", param.ImageID);
        }

    }
}
