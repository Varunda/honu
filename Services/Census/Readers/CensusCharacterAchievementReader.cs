using Newtonsoft.Json.Linq;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Census;

namespace watchtower.Services.Census.Readers {

    public class CensusCharacterAchievementReader : ICensusReader<CharacterAchievement> {

        public override CharacterAchievement? ReadEntry(JToken token) {
            CharacterAchievement cach = new CharacterAchievement();

            cach.CharacterID = token.GetRequiredString("character_id");
            cach.AchievementID = token.GetRequiredInt32("achievement_id");
            cach.EarnedCount = token.GetInt32("earned_count", 0);
            cach.StartDate = token.CensusTimestamp("start");

            int finishTime = token.GetInt32("finish", 0);
            if (finishTime > 0) {
                cach.FinishDate = token.CensusTimestamp("finish");
            }
            cach.LastSaveDate = token.CensusTimestamp("last_save");

            return cach;
        }

    }
}
