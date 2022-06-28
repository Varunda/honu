using Npgsql;
using System.Threading.Tasks;

namespace watchtower.Services.Db.Patches {

    [Patch]
    public class Patch57ItemCategoryAndType : IDbPatch {
        public int MinVersion => 57;
        public string Name => "create item_category and item_type";

        public async Task Execute(IDbHelper helper) {
            using NpgsqlConnection conn = helper.Connection();
            using NpgsqlCommand cmd = await helper.Command(conn, @"
                CREATE TABLE IF NOT EXISTS item_type (
                    id int NOT NULL PRIMARY KEY,
                    name varchar NOT NULL,
                    code varchar NOT NULL
                );

                CREATE TABLE IF NOT EXISTS item_category (
                    id int NOT NULL PRIMARY KEY,
                    name varchar NOT NULL
                );
            ");

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

    }
}
