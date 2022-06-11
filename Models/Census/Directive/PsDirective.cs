
using System;

namespace watchtower.Models.Census {

    public class PsDirective {

        /// <summary>
        ///     Unique ID of this directive
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        ///     ID of the <see cref="DirectiveTree"/> this directive is in
        /// </summary>
        public int TreeID { get; set; }

        /// <summary>
        ///     Which tier this directive is in. Not really an ID, as they aren't unique for <see cref="DirectiveTier"/>s
        /// </summary>
        public int TierID { get; set; }

        /// <summary>
        ///     What objective must be completed to progress this directive. From Census, this would never be null, but since
        ///     I haven't figured out a way to get it from DataSources, it's nullable for those entries
        /// </summary>
        public int? ObjectiveSetID { get; set; }

        /// <summary>
        ///     English name of the directive
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        ///     English description of the directive
        /// </summary>
        public string Description { get; set; } = "";

        public int ImageSetID { get; set; }

        public int ImageID { get; set; }

        public override bool Equals(object? obj) {
            return obj is PsDirective directive &&
                   ID == directive.ID &&
                   TreeID == directive.TreeID &&
                   TierID == directive.TierID &&
                   ObjectiveSetID == directive.ObjectiveSetID &&
                   Name == directive.Name &&
                   Description == directive.Description &&
                   ImageSetID == directive.ImageSetID &&
                   ImageID == directive.ImageID;
        }

        public override int GetHashCode() {
            return HashCode.Combine(ID, TreeID, TierID, ObjectiveSetID, Name, Description, ImageSetID, ImageID);
        }
    }

}