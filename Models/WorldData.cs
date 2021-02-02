using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Models {

    /// <summary>
    /// All the data for a single world/server
    /// </summary>
    public class WorldData {

        /// <summary>
        /// ID of the world this data is for
        /// </summary>
        public string WorldID { get; set; } = "";

        /// <summary>
        /// Name of the world this data is for
        /// </summary>
        public string WorldName { get; set; } = "";

        /// <summary>
        /// TR specific data
        /// </summary>
        public FactionData TR { get; set; } = new FactionData();

        /// <summary>
        /// NC specific data
        /// </summary>
        public FactionData NC { get; set; } = new FactionData();

        /// <summary>
        /// VS specific data
        /// </summary>
        public FactionData VS { get; set; } = new FactionData();

    }

    /// <summary>
    /// Data about a single faction
    /// </summary>
    public class FactionData {

        /// <summary>
        /// ID of the faction
        /// </summary>
        public string FactionID { get; set; } = "";

        /// <summary>
        /// Name of the faction
        /// </summary>
        public string FactionName { get; set; } = "";

        /// <summary>
        /// The top killers for that faction
        /// </summary>
        public KillBlock PlayerKills { get; set; } = new KillBlock();

        /// <summary>
        /// Top outfits for average kills per player
        /// </summary>
        public OutfitKillBlock OutfitKills { get; set; } = new OutfitKillBlock();

        /// <summary>
        /// Top outfits for heals for that faction
        /// </summary>
        public Block OutfitHeals { get; set; } = new Block();

        /// <summary>
        /// Top players for heals for that faction
        /// </summary>
        public Block PlayerHeals { get; set; } = new Block();

        /// <summary>
        /// Top outfits for revives for that faction
        /// </summary>
        public Block OutfitRevives { get; set; } = new Block();

        /// <summary>
        /// Top players for revives for that faction
        /// </summary>
        public Block PlayerRevives { get; set; } = new Block();

        /// <summary>
        /// Top outfits for resupplies for that faction
        /// </summary>
        public Block OutfitResupplies { get; set; } = new Block();

        /// <summary>
        /// Top players for resupplies for that faction
        /// </summary>
        public Block PlayerResupplies { get; set; } = new Block();

    }

}
