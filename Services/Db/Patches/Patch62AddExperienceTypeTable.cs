using Npgsql;
using System.Threading.Tasks;

namespace watchtower.Services.Db.Patches {

    [Patch]
    public class Patch62AddExperienceTypeTable : IDbPatch {
        public int MinVersion => 62;
        public string Name => "add experience_type table";

        public async Task Execute(IDbHelper helper) {
            using NpgsqlConnection conn = helper.Connection();
            using NpgsqlCommand cmd = await helper.Command(conn, @"
                CREATE TABLE IF NOT EXISTS experience_type (
                    id int NOT NULL PRIMARY KEY,
                    name varchar NOT NULL,
                    amount double precision NOT NULL
                );
            ");

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }
    }
}
