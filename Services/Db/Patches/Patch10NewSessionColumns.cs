using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Services.Db.Patches {

    [Patch]
    public class Patch10NewSessionColumns : IDbPatch {

        public int MinVersion => 10;
        public string Name => "Add team_id and outfit_id to wt_session";

        public async Task Execute(IDbHelper helper) {
            using NpgsqlConnection conn = helper.Connection();
            using NpgsqlCommand cmd = await helper.Command(conn, @"
                ALTER TABLE wt_session
                    ADD IF NOT EXISTS team_id smallint NOT NULL DEFAULT 0;

                ALTER TABLE wt_session
                    ADD IF NOT EXISTS outfit_id varchar NULL DEFAULT 0;
            ");

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

    }
}
