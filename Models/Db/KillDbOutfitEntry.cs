using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Models.Db {

    public class KillDbOutfitEntry {

        /// <summary>
        /// ID of the outfit 
        /// </summary>
        public string OutfitID { get; set; } = "";

        /// <summary>
        /// How many kills in total the outfit had when this entry was created
        /// </summary>
        public int Kills { get; set; }

        /// <summary>
        /// How many deaths in total the outfit had when this entry was created
        /// </summary>
        public int Deaths { get; set; }

        /// <summary>
        /// How many members were online when this entry was created
        /// </summary>
        public int Members { get; set; }

    }
}
