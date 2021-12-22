using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models.Census;
using watchtower.Models.Db;

namespace watchtower.Services.Db.Readers {

    public class DirectiveTreeReader : IDataReader<DirectiveTree> {

        public override DirectiveTree? ReadEntry(NpgsqlDataReader reader) {
            DirectiveTree tree = new DirectiveTree();

            tree.ID = reader.GetInt32("id");
            tree.CategoryID = reader.GetInt32("category_id");
            tree.Name = reader.GetString("name");
            tree.ImageSetID = reader.GetInt32("image_set_id");
            tree.ImageID = reader.GetInt32("image_id");

            return tree;
        }

    }
}
