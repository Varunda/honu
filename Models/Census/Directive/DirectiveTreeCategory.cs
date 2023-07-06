
using System;

namespace watchtower.Models.Census {

    public class DirectiveTreeCategory : IKeyedObject {

        /// <summary>
        ///     Unique ID of the tree
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        ///     English name
        /// </summary>
        public string Name { get; set; } = "";

        public override bool Equals(object? obj) {
            return obj is DirectiveTreeCategory category &&
                   ID == category.ID &&
                   Name == category.Name;
        }

        public override int GetHashCode() {
            return HashCode.Combine(ID, Name);
        }
    }

}