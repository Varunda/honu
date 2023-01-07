using System.Collections.Generic;
using watchtower.Models.Census;
using watchtower.Models.Events;

namespace watchtower.Models.Api {

    public class AchievementEarnedBlock {

        public List<AchievementEarnedEvent> Events { get; set; } = new();

        public List<Achievement> Achievements { get; set; } = new();

    }
}
