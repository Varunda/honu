using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Census;
using watchtower.Models.Db;

namespace watchtower.Services.Db.Readers {

    public class DirectiveReader : IDataReader<PsDirective> {

        public override PsDirective? ReadEntry(NpgsqlDataReader reader) {
            PsDirective dir = new PsDirective();

            dir.ID = reader.GetInt32("id");
            dir.TreeID = reader.GetInt32("tree_id");
            dir.TierID = reader.GetInt32("tier_id");
            dir.ObjectiveSetID = reader.GetNullableInt32("objective_set_id");
            dir.Name = reader.GetString("name");
            dir.Description = reader.GetString("description");
            dir.ImageSetID = reader.GetInt32("image_set_id");
            dir.ImageID = reader.GetInt32("image_id");

            return dir;
        }

    }
}
