using System.Collections.Generic;

namespace watchtower.Models.Census {

    public class CharacterAchievementBlock {

        public string CharacterID { get; set; } = "";

        public List<CharacterAchievement> Entries { get; set; } = new();

        public List<Achievement> Achievements { get; set; } = new();

        public List<PsObjective> Objectives { get; set; } = new();

        public List<ObjectiveType> ObjectiveTypes { get; set; } = new();

        public List<PsItem> Items { get; set; } = new();

        public List<PsVehicle> Vehicles { get; set; } = new();

        public List<ExperienceAwardType> AwardTypes { get; set; } = new();

    }
}
