using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Models.CharacterViewer.CharacterStats {

    /// <summary>
    /// Base stat that all other generated stats come from
    /// </summary>
    public class CharacterStatBase {

        /// <summary>
        /// ID of the character this stat is for
        /// </summary>
        public string CharacterID { get; set; } = "";

        /// <summary>
        /// Name of the stat
        /// </summary>
        public string Name { get; } = "";

        /// <summary>
        /// An optional description of what the stat is
        /// </summary>
        public string? Description { get; }

        /// <summary>
        /// What value this stat is
        /// </summary>
        public double Value { get; }

        public CharacterStatBase(string name) {
            Name = name;
        }

        public CharacterStatBase(string name, string description) {
            Name = name;
            Description = description;
        }

        public CharacterStatBase(string name, double value) {
            Name = name;
            Value = value;
        }

        public CharacterStatBase(string name, string description, double value) {
            Name = name;
            Description = description;
            Value = value;
        }


    }
}
