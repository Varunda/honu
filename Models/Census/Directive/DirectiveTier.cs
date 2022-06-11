
using System;

namespace watchtower.Models.Census {

    public class DirectiveTier {

        /// <summary>
        ///     ID of the <see cref="DirectiveTree"/> this tier is in
        /// </summary>
        public int TreeID { get; set; }

        /// <summary>
        ///     NOT UNIQUE, ID of the <see cref="DirectiveTier"/>
        ///     Unique within the tree. compare the TreeID and TierID to match
        /// </summary>
        public int TierID { get; set; }

        public int? RewardSetID { get; set; }

        /// <summary>
        ///     How many directive points a character will be granted from completing this tier
        /// </summary>
        public int DirectivePoints { get; set; }

        /// <summary>
        ///     How many directives within this tier must be completed to count as finished
        /// </summary>
        public int CompletionCount { get; set; }

        /// <summary>
        ///     English name
        /// </summary>
        public string Name { get; set; } = "";

        public int ImageSetID { get; set; }

        public int ImageID { get; set; }

        public override bool Equals(object? obj) {
            return obj is DirectiveTier tier &&
                   TreeID == tier.TreeID &&
                   TierID == tier.TierID &&
                   RewardSetID == tier.RewardSetID &&
                   DirectivePoints == tier.DirectivePoints &&
                   CompletionCount == tier.CompletionCount &&
                   Name == tier.Name &&
                   ImageSetID == tier.ImageSetID &&
                   ImageID == tier.ImageID;
        }

        public override int GetHashCode() {
            return HashCode.Combine(TreeID, TierID, RewardSetID, DirectivePoints, CompletionCount, Name, ImageSetID, ImageID);
        }
    }

}