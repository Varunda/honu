
namespace watchtower.Models.Census {

    public class DirectiveTier {

        public int TreeID { get; set; }

        public int TierID { get; set; }

        public int? RewardSetID { get; set; }

        public int DirectivePoints { get; set; }

        public int CompletionCount { get; set; }

        public string Name { get; set; } = "";

        public int ImageSetID { get; set; }

        public int ImageID { get; set; }

    }

}