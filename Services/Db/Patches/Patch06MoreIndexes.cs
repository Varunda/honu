using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Services.Db.Patches {

    [Patch]
    public class Patch06MoreIndexes : IDbPatch {

        public int MinVersion => 6;
        public string Name => "Add more indexes";

        public async Task Execute(IDbHelper helper) {
            using NpgsqlConnection conn = helper.Connection();
            using NpgsqlCommand cmd = await helper.Command(conn, @"
                CREATE INDEX IF NOT EXISTS idx_wt_session_character_id ON wt_session(character_id);

                CREATE INDEX IF NOT EXISTS idx_wt_outfit_faction_id ON wt_outfit(faction_id);

                CREATE INDEX IF NOT EXISTS idx_wt_character_outfit_id ON wt_character(outfit_id);
            ");

            await cmd.ExecuteNonQueryAsync();

            /*
            if (await helper.HasIndex("wt_session", "idx_wt_session_character_id") == false) {
                using NpgsqlConnection conn = helper.Connection();
                using NpgsqlCommand cmd = await helper.Command(conn, @"
                    CREATE INDEX idx_wt_session_character_id ON wt_session(character_id);
                ");
                await conn.CloseAsync();
            }

            if (await helper.HasIndex("wt_outfit", "idx_wt_outfit_faction_id") == false) {
                using NpgsqlConnection conn = helper.Connection();
                using NpgsqlCommand cmd = await helper.Command(conn, @"
                    CREATE INDEX idx_wt_outfit_faction_id ON wt_outfit(faction_id);
                ");
                await conn.CloseAsync();
            }

            if (await helper.HasIndex("wt_character", "idx_wt_character_outfit_id") == false) {
                using NpgsqlConnection conn = helper.Connection();
                using NpgsqlCommand cmd = await helper.Command(conn, @"
                    CREATE INDEX idx_wt_character_outfit_id ON wt_character(outfit_id);
                ");
                await conn.CloseAsync();
            }
            */
        }

    }
}
