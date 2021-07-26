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
        public short WorldID { get; set; }

        /// <summary>
        /// Name of the world this data is for
        /// </summary>
        public string WorldName { get; set; } = "";

        /// <summary>
        /// Timestamp of when this data was generated
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// How many characters are on each continent
        /// </summary>
        public ContinentCount ContinentCount { get; set; } = new ContinentCount();

        /// <summary>
        /// Count of how many are currently online in this world
        /// </summary>
        public int OnlineCount { get; set; } = 0;

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

        /// <summary>
        /// Top sundies that have been placed on the continent
        /// </summary>
        public SpawnEntries TopSpawns { get; set; } = new SpawnEntries();

        public FactionFocus FactionFocus { get; set; } = new FactionFocus();

    }

    public class FactionFocus {

        public FactionFocusEntry VS { get; set; } = new FactionFocusEntry();

        public FactionFocusEntry NC { get; set; } = new FactionFocusEntry();

        public FactionFocusEntry TR { get; set; } = new FactionFocusEntry();
    }

    public class FactionFocusEntry {

        public int VsKills { get; set; }

        public int NcKills { get; set; }

        public int TrKills { get; set;}

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

        /// <summary>
        /// Top players for spawns for that faction
        /// </summary>
        public Block PlayerSpawns { get; set; } = new Block();

        /// <summary>
        /// Top outfits for spawns for that faction
        /// </summary>
        public Block OutfitSpawns { get; set; } = new Block();

        /// <summary>
        /// Top players for vehicle kills in this faction
        /// </summary>
        public Block PlayerVehicleKills { get; set; } = new Block();

        /// <summary>
        /// Top outfits for vehicle kills in this faction
        /// </summary>
        public Block OutfitVehicleKills { get; set; } = new Block();

        /// <summary>
        /// Top weapons used for this faction
        /// </summary>
        public WeaponKillsBlock WeaponKills { get; set; } = new WeaponKillsBlock();

        /// <summary>
        /// Total number of kills a faction has gotten
        /// </summary>
        public int TotalKills { get; set; }

        /// <summary>
        /// Total number of assists a faction has
        /// </summary>
        public int TotalAssists { get; set; }

        /// <summary>
        /// Total number of deaths a faction has gotten
        /// </summary>
        public int TotalDeaths { get; set; }

    }

}
