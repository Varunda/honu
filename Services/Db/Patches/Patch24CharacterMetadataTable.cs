using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Services.Db.Patches {

    [Patch]
    public class Patch24CharacterMetadataTable : IDbPatch {

        public int MinVersion => 24;
        public string Name => "Add character_metadata table";

        public async Task Execute(IDbHelper helper) {
            using NpgsqlConnection conn = helper.Connection();
            using NpgsqlCommand cmd = await helper.Command(conn, @"
                CREATE TABLE IF NOT EXISTS character_metadata (
                    id varchar NOT NULL PRIMARY KEY,
                    last_updated timestamptz NOT NULL,
                    not_found_count int NOT NULL
                );
            ");

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }
    }
}
