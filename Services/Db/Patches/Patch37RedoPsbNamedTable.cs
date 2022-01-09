using Npgsql;
using System.Threading.Tasks;

namespace watchtower.Services.Db.Patches {

    [Patch]
    public class Patch37AddPsbNamedTable : IDbPatch {
        public int MinVersion => 37;
        public string Name => "Remake psb_named table";

        public async Task Execute(IDbHelper helper) {
            using NpgsqlConnection conn = helper.Connection();
            using NpgsqlCommand cmd = await helper.Command(conn, @"
                DROP TABLE IF EXISTS psb_named;

                CREATE TABLE psb_named (
                    id bigint NOT NULL PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
                    tag varchar NULL,
                    name varchar NOT NULL,
                    vs_id varchar NULL,
                    nc_id varchar NULL,
                    tr_id varchar NULL,
                    ns_id varchar NULL,
                    notes varchar NULL,

                    UNIQUE (tag, name) 
                );
            ");

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

    }
}
