using System;

namespace watchtower.Models.Census {

    public class CharacterAchievement {

        public string CharacterID { get; set; } = "";

        public int AchievementID { get; set; }

        public int EarnedCount { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? FinishDate { get; set; }

        public DateTime LastSaveDate { get; set; }

    }
}
