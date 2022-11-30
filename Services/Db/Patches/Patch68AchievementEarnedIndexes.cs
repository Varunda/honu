using Npgsql;
using System.Threading.Tasks;

namespace watchtower.Services.Db.Patches {

    [Patch]
    public class Patch68AchievementEarnedIndexes : IDbPatch {

        public int MinVersion => 68;
        public string Name => "create indexes on achievement_earned";

        public async Task Execute(IDbHelper helper) {
            using NpgsqlConnection conn = helper.Connection();
            using NpgsqlCommand cmd = await helper.Command(conn, @"
                CREATE INDEX IF NOT EXISTS idx_achievement_earned_character_id ON achievement_earned(character_id);

                CREATE INDEX IF NOT EXISTS idx_achievement_earned_timestamp ON achievement_earned(timestamp);
            ");

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

    }
}
