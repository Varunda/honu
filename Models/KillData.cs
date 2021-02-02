using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Models {

    public class KillBlock {

        public List<KillData> Entries { get; set; } = new List<KillData>();

    }

    /// <summary>
    /// Represents the kill data for a single character
    /// </summary>
    public class KillData {

        /// <summary>
        /// ID of the character
        /// </summary>
        public string ID { get; set; } = "";

        /// <summary>
        /// Name of the character. Includes the tag if they are in an outfit
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// How many kills they got
        /// </summary>
        public int Kills { get; set; }

        /// <summary>
        /// How many deaths (revived of course) they have
        /// </summary>
        public int Deaths { get; set; }

    }
}
