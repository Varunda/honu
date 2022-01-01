using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Services.Db.Patches {

    [Patch]
    public class Patch4AddItemTable : IDbPatch {

        public int MinVersion => 4;
        public string Name => "Add wt_item";

        public Task Execute(IDbHelper helper) {
            return Task.CompletedTask;
            // Table is deleted then recreated in Patch 32
            /*
            using NpgsqlConnection conn = helper.Connection();
            using NpgsqlCommand cmd = await helper.Command(conn, @"
                CREATE TABLE IF NOT EXISTS wt_item (
                    id varchar PRIMARY KEY NOT NULL,
                    category_id int NOT NULL,
                    type_id int NOT NULL,
                    name varchar NOT NULL
                );
            ");

            await cmd.ExecuteNonQueryAsync();
            */
        }

    }
}
