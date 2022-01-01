using Npgsql;
using System.Threading.Tasks;

namespace watchtower.Services.Db.Patches {

    [Patch]
    public class Patch31AddAchievementTable : IDbPatch {

        public int MinVersion => 31;

        public string Name => "add achievement table";

        public async Task Execute(IDbHelper helper) {
            using NpgsqlConnection conn = helper.Connection();
            using NpgsqlCommand cmd = await helper.Command(conn, @"
                CREATE TABLE IF NOT EXISTS achievement (
                    id int NOT NULL PRIMARY KEY,
                    item_id int NULL,
                    objective_group_id int NOT NULL,
                    reward_id int NULL,
                    repeatable boolean NOT NULL,
                    name varchar NOT NULL,
                    description varchar NOT NULL,
                    image_set_id int NOT NULL,
                    image_id int NOT NULL
                );
            ");

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

    }
}
