using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Services.Db.Patches {

    [Patch]
    public class Patch21AddMoreOutfitColumns : IDbPatch {

        public int MinVersion => 21;

        public string Name => "Add more columns from /outfit";

        public async Task Execute(IDbHelper helper) {
            using NpgsqlConnection conn = helper.Connection();
            using NpgsqlCommand cmd = await helper.Command(conn, @"
                ALTER TABLE wt_outfit
	                ADD COLUMN IF NOT EXISTS time_create timestamptz NULL;

                ALTER TABLE wt_outfit
	                ADD COLUMN IF NOT EXISTS leader_id varchar NOT NULL DEFAULT '';

                ALTER TABLE wt_outfit
                    ADD COLUMN IF NOT EXISTS member_count int NOT NULL DEFAULT 0;
            ");

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

    }
}
