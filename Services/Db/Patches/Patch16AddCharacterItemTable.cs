using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Services.Db.Patches {

    [Patch]
    public class Patch16AddCharacterItemTable : IDbPatch {

        public int MinVersion => 16;
        public string Name => "Add character item table";

        public async Task Execute(IDbHelper helper) {
            using NpgsqlConnection conn = helper.Connection();
            using NpgsqlCommand cmd = await helper.Command(conn, @"
                CREATE TABLE IF NOT EXISTS character_items (
                    character_id varchar NOT NULL,
                    item_id varchar NOT NULL,
                    account_level bool NOT NULL,
                    stack_count int NULL
                );

                CREATE INDEX IF NOT EXISTS idx_character_items_character_id ON character_items (character_id);

                CREATE INDEX IF NOT EXISTS idx_character_items_item_id ON character_items (item_id);
            ");

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

    }
}
