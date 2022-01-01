using Npgsql;
using System.Data;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Census;

namespace watchtower.Services.Db.Readers {

    public class CharacterAchievementReader : IDataReader<CharacterAchievement> {

        public override CharacterAchievement? ReadEntry(NpgsqlDataReader reader) {
            CharacterAchievement cach = new CharacterAchievement();

            cach.CharacterID = reader.GetString("character_id");
            cach.AchievementID = reader.GetInt32("achievement_id");
            cach.EarnedCount = reader.GetInt32("earned_count");
            cach.StartDate = reader.GetDateTime("start_date");
            cach.FinishDate = reader.GetNullableDateTime("finish_date");
            cach.LastSaveDate = reader.GetDateTime("last_save_date");

            return cach;
        }

    }
}
