
using System;

namespace watchtower.Models.Census {

    public class DirectiveTree {

        /// <summary>
        ///     Unique ID of the tree
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        ///     ID of the <see cref="DirectiveTreeCategory"/> this tree is in
        /// </summary>
        public int CategoryID { get; set; }

        /// <summary>
        ///     English name
        /// </summary>
        public string Name { get; set; } = "";

        public int ImageSetID { get; set; }

        public int ImageID { get; set; }

        public override bool Equals(object? obj) {
            return obj is DirectiveTree tree &&
                   ID == tree.ID &&
                   CategoryID == tree.CategoryID &&
                   Name == tree.Name &&
                   ImageSetID == tree.ImageSetID &&
                   ImageID == tree.ImageID;
        }

        public override int GetHashCode() {
            return HashCode.Combine(ID, CategoryID, Name, ImageSetID, ImageID);
        }

    }

}