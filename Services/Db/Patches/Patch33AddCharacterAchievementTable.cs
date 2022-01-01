using Npgsql;
using System.Threading.Tasks;

namespace watchtower.Services.Db.Patches {

    [Patch]
    public class Patch33AddCharacterAchievementTable : IDbPatch {

        public int MinVersion => 33;

        public string Name => "add character_achievement tabl3";

        public async Task Execute(IDbHelper helper) {
            using NpgsqlConnection conn = helper.Connection();
            using NpgsqlCommand cmd = await helper.Command(conn, @"
                CREATE TABLE IF NOT EXISTS character_achievement (
                    character_id varchar NOT NULL,
                    achievement_id int NOT NULL,
                    earned_count int NOT NULL,
                    start_date timestamptz NOT NULL,
                    finish_date timestamptz NULL,
                    last_save_date timestamptz NOT NULL,

                    PRIMARY KEY (character_id, achievement_id)
                );
            ");

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

    }
}
