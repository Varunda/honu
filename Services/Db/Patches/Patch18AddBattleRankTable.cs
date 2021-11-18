using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Services.Db.Patches {

    [Patch]
    public class Patch18AddBattleRankTable : IDbPatch {

        public int MinVersion => 18;

        public string Name => "Add battle rank patch";

        public async Task Execute(IDbHelper helper) {
            using NpgsqlConnection conn = helper.Connection();
            using NpgsqlCommand cmd = await helper.Command(conn, @"
                CREATE TABLE IF NOT EXISTS battle_rank (
                    character_id varchar NOT NULL,
                    rank int NOT NULL,
                    timestamp timestamptz NOT NULL DEFAULT (NOW() at time zone 'utc')
                );
            ");

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

    }
}
