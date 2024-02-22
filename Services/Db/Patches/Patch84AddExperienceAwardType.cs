using Npgsql;
using System.Threading.Tasks;

namespace watchtower.Services.Db.Patches {

    [Patch]
    public class Patch84AddExperienceAwardType : IDbPatch {

        public int MinVersion => 84;
        public string Name => "add experience_award_type";

        public async Task Execute(IDbHelper helper) {
            using NpgsqlConnection conn = helper.Connection(Dbs.EVENTS);
            using NpgsqlCommand cmd = await helper.Command(conn, @"
                CREATE TABLE IF NOT EXISTS experience_award_type (
                    id INT PRIMARY KEY NOT NULL,
                    name VARCHAR NOT NULL
                );

                ALTER TABLE experience_type
                    ADD COLUMN IF NOT EXISTS award_type_id int NOT NULL DEFAULT 0;
            ");

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

    }
}
