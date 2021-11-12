using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Services.Db.Patches {

    [Patch]
    public class Patch17AddCharacterStatTable : IDbPatch {

        public int MinVersion => 17;
        public string Name => "Add character stat table";

        public async Task Execute(IDbHelper helper) {
            using NpgsqlConnection conn = helper.Connection();
            using NpgsqlCommand cmd = await helper.Command(conn, @"
                CREATE TABLE IF NOT EXISTS character_stats (
                    character_id varchar NOT NULL,
                    stat_name varchar NOT NULL,
                    profile_id int NOT NULL,
                    value_forever int NOT NULL,
                    value_monthly int NOT NULL,
                    value_weekly int NOT NULL,
                    value_daily int NOT NULL,
                    value_max_one_life int NOT NULL,
                    timestamp timestamptz NOT NULL DEFAULT (NOW() at time zone 'utc')
                );

                CREATE INDEX IF NOT EXISTS idx_character_stats_character_id ON character_stats (character_id);
                CREATE INDEX IF NOT EXISTS idx_character_stats_stat_name ON character_stats (stat_name);
            ");

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

    }
}
