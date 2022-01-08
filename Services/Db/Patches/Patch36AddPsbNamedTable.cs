using Npgsql;
using System.Threading.Tasks;

namespace watchtower.Services.Db.Patches {

    [Patch]
    public class Patch36AddPsbNamedTable : IDbPatch {
        public int MinVersion => 36;
        public string Name => "Add psb_named finish partial index";

        public async Task Execute(IDbHelper helper) {
            using NpgsqlConnection conn = helper.Connection();
            using NpgsqlCommand cmd = await helper.Command(conn, @"
                CREATE TABLE IF NOT EXISTS psb_named (
                    tag varchar NULL,
                    name varchar NOT NULL,
                    vs_id varchar NULL,
                    nc_id varchar NULL,
                    tr_id varchar NULL,
                    ns_id varchar NULL,
                    notes varchar NULL,

                    PRIMARY KEY (tag, name) 
                );
            ");

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

    }
}
