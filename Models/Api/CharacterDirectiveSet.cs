
using System.Collections.Generic;
using watchtower.Models.Census;
using watchtower.Models.CharacterViewer.WeaponStats;

namespace watchtower.Models.Api {

    /// <summary>
    ///     Expanded class that contains all the information about the directive progress of a character
    /// </summary>  
    public class CharacterDirectiveSet {

        /// <summary>
        ///     ID of the character this directive progress is for
        /// </summary>
        public string CharacterID { get; set; } = "";

        /// <summary>
        ///     Directives are organized into their categories
        /// </summary>
        public List<ExpandedCharacterDirectiveCategory> Categories { get; set; } = new();

    }

    /// <summary>
    ///     Expanded class that represents the per directive category progress a character has made
    /// </summary>
    public class ExpandedCharacterDirectiveCategory {

        /// <summary>
        ///     ID of the category
        /// </summary>
        public int CategoryID { get; set; }

        public DirectiveTreeCategory? Category { get; set; }

        public List<ExpandedCharacterDirectiveTree> Trees { get; set; } = new();

    }

    public class ExpandedCharacterDirectiveTree {

        public CharacterDirectiveTree Entry { get; set; } = new();

        public DirectiveTree? Tree { get; set; }

        public List<ExpandedCharacterDirectiveTier> Tiers { get; set; } = new();

    }

    public class ExpandedCharacterDirectiveTier {

        public int TierID { get; set; }

        public CharacterDirectiveTier? Entry { get; set; } = new();

        public DirectiveTier? Tier { get; set; }

        public List<ExpandedCharacterDirective> Directives { get; set; } = new();

    }

    public class ExpandedCharacterDirective {

        public CharacterDirective? Entry { get; set; }

        public PsDirective Directive { get; set; } = new();

        public string Name { get; set; } = "";

        public string Description { get; set; } = "";

        public int? Progress { get; set; }

        public int? Goal { get; set; }

        public CharacterDirectiveObjective? CharacterObjective { get; set; }

        public PsObjective? Objective { get; set; }

        public ObjectiveType? ObjectiveType { get; set; }

        public Achievement? Achievement { get; set; }

        public PsObjective? AchievementObjective { get; set; }

        public ObjectiveType? AchievementObjectiveType { get; set; }

        public WeaponStatEntry? WeaponStatEntry { get; set; }

        public string ObjectiveSource { get; set; } = "";

    }

}