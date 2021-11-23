using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Services.Db.Patches {

    [Patch]
    public class Patch22AddOutfitReportTable : IDbPatch {

        public int MinVersion => 22;

        public string Name => "Add outfit report table";

        public async Task Execute(IDbHelper helper) {
            using NpgsqlConnection conn = helper.Connection();
            using NpgsqlCommand cmd = await helper.Command(conn, @"
                CREATE TABLE IF NOT EXISTS outfit_report (
                    id bigint NOT NULL PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
                    generator varchar NOT NULL,
                    timestamp timestamptz NOT NULL DEFAULT (NOW() at time zone 'utc'),
                    period_start timestamptz NOT NULL,
                    period_end timestamptz NOT NULL
                );
            ");

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

    }
}
