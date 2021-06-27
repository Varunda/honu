using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Services.Db.Patches {

    [Patch]
    public class Patch0CreateMetadataTable : IDbPatch {

        public async Task Execute(IDbHelper helper) {
            using NpgsqlConnection conn = helper.Connection();
            using NpgsqlCommand cmd = await helper.Command(conn, @"
                CREATE TABLE IF NOT EXISTS metadata (
                    name varchar NOT NULL PRIMARY KEY,
                    value varchar NOT NULL
                );
            ");

            await cmd.ExecuteNonQueryAsync();
        }

        public int MinVersion => 0;

        public string Name => $"CreateMetadataTable";

    }
}
