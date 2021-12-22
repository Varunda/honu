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

    public class DirectiveTreeCategoryReader : IDataReader<DirectiveTreeCategory> {

        public override DirectiveTreeCategory? ReadEntry(NpgsqlDataReader reader) {
            DirectiveTreeCategory cat = new DirectiveTreeCategory();

            cat.ID = reader.GetInt32("id");
            cat.Name = reader.GetString("name");

            return cat;
        }

    }
}
