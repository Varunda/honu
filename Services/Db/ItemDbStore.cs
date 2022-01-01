using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Census;

namespace watchtower.Services.Db {

    public class ItemDbStore : BaseStaticDbStore<PsItem> {

        public ItemDbStore(ILoggerFactory loggerFactory,
            IDataReader<PsItem> reader, IDbHelper helper)
            : base("wt_item", loggerFactory, reader, helper) { }

        internal override void SetupUpsertCommand(NpgsqlCommand cmd, PsItem param) {
            cmd.CommandText = @"
                INSERT INTO wt_item (
                    id, category_id, type_id, is_vehicle_weapon, name, description, faction_id, image_id, image_set_id
                ) VALUES (
                    @ID, @CategoryID, @TypeID, @IsVehicleWeapon, @Name, @Description, @FactionID, @ImageID, @ImageSetID
                ) ON CONFLICT (id) DO
                    UPDATE SET
                        category_id = @CategoryID,
                        type_id = @TypeID,
                        is_vehicle_weapon = @IsVehicleWeapon,
                        name = @Name,
                        description = @Description,
                        faction_id = @FactionID,
                        image_id = @ImageID,
                        image_set_id = @ImageSetID
            ";

            cmd.AddParameter("ID", param.ID);
            cmd.AddParameter("CategoryID", param.CategoryID);
            cmd.AddParameter("TypeID", param.TypeID);
            cmd.AddParameter("IsVehicleWeapon", param.IsVehicleWeapon);
            cmd.AddParameter("Name", param.Name);
            cmd.AddParameter("Description", param.Description);
            cmd.AddParameter("FactionID", param.FactionID);
            cmd.AddParameter("ImageID", param.ImageID);
            cmd.AddParameter("ImageSetID", param.ImageSetID);
        }

    }
}
