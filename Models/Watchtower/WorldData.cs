using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models.Api;
using watchtower.Models.Census;
using watchtower.Models.Db;
using watchtower.Models.Health;
using watchtower.Models.Watchtower;

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
        /// How many minutes back this data goes
        /// </summary>
        public int Duration { get; set; }

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

        /// <summary>
        ///     Game of tag stuff
        /// </summary>
        public List<WorldTagEntry> TagEntries { get; set; } = new List<WorldTagEntry>();

        /// <summary>
        ///     List of all reconnects that have occured within the time period
        /// </summary>
        public List<RealtimeReconnectEntry> Reconnects { get; set; } = new List<RealtimeReconnectEntry>();

        /// <summary>
        ///     Health entries of the streams
        /// </summary>
        public List<CensusRealtimeHealthEntry> RealtimeHealth { get; set; } = new();

        /// <summary>
        ///     How many seconds behind Honu is when processing events
        /// </summary>
        public int ProcessLag { get; set; } = 0;

        /// <summary>
        ///     Last error that was thrown while trying to generate this data
        /// </summary>
        public ProblemDetails? LastError { get; set; } = null;

        /// <summary>
        ///     List of fights current happening on this world
        /// </summary>
        public List<RealtimeDataFight> Fights { get; set; } = new();

        /// <summary>
        ///     Population changes over time for the last period of time
        /// </summary>
        public List<WorldZonePopulation> Population { get; set; } = new();

        /// <summary>
        ///     estimates of the vehicle usage
        /// </summary>
        public VehicleUsageData VehicleUsage { get; set; } = new();

    }

    public class FactionFocusEntry {

        public int VsKills { get; set; }

        public int NcKills { get; set; }

        public int TrKills { get; set; }

        public int TotalKills { get; set; }

    }

    /// <summary>
    ///     data about a single faction
    /// </summary>
    public class FactionData {

        /// <summary>
        ///     ID of the faction
        /// </summary>
        public short FactionID { get; set; }

        /// <summary>
        ///     name of the faction
        /// </summary>
        public string FactionName { get; set; } = "";

        /// <summary>
        ///     the top killers for that faction
        /// </summary>
        public KillBlock PlayerKills { get; set; } = new KillBlock();

        /// <summary>
        ///     top outfits for average kills per player
        /// </summary>
        public OutfitKillBlock OutfitKills { get; set; } = new OutfitKillBlock();

        /// <summary>
        ///     top outfits for heals for that faction
        /// </summary>
        public Block OutfitHeals { get; set; } = new Block();

        /// <summary>
        ///     top players for heals for that faction
        /// </summary>
        public Block PlayerHeals { get; set; } = new Block();

        /// <summary>
        ///     top outfits for revives for that faction
        /// </summary>
        public Block OutfitRevives { get; set; } = new Block();

        /// <summary>
        ///     top players for revives for that faction
        /// </summary>
        public Block PlayerRevives { get; set; } = new Block();

        /// <summary>
        ///     top outfits for resupplies for that faction
        /// </summary>
        public Block OutfitResupplies { get; set; } = new Block();

        /// <summary>
        ///     top players for resupplies for that faction
        /// </summary>
        public Block PlayerResupplies { get; set; } = new Block();

        /// <summary>
        ///     top players for spawns for that faction
        /// </summary>
        public Block PlayerSpawns { get; set; } = new Block();

        /// <summary>
        ///     top outfits for spawns for that faction
        /// </summary>
        public Block OutfitSpawns { get; set; } = new Block();

        /// <summary>
        ///     top players for vehicle kills in this faction
        /// </summary>
        public Block PlayerVehicleKills { get; set; } = new Block();

        /// <summary>
        ///     top outfits for vehicle kills in this faction
        /// </summary>
        public Block OutfitVehicleKills { get; set; } = new Block();

        /// <summary>
        ///     top players for shield repairs for this faction
        /// </summary>
        public Block PlayerShieldRepair { get; set; } = new Block();

        /// <summary>
        ///     top outfits for shield repairs for this faction
        /// </summary>
        public Block OutfitShieldRepair { get; set; } = new Block();

        /// <summary>
        ///     top weapons used for this faction
        /// </summary>
        public WeaponKillsBlock WeaponKills { get; set; } = new WeaponKillsBlock();

        /// <summary>
        ///     outfits currently online when the data was created
        /// </summary>
        public OutfitsOnline Outfits { get; set; } = new OutfitsOnline();

        /// <summary>
        ///     faction focus based on Death events
        /// </summary>
        public FactionFocusEntry FactionFocus { get; set; } = new FactionFocusEntry();

        /// <summary>
        ///     how many players are using each class
        /// </summary>
        public RealtimeClassUsage ClassUsage { get; set; } = new();

        /// <summary>
        ///     total number of kills a faction has gotten
        /// </summary>
        public int TotalKills { get; set; }

        /// <summary>
        ///     total number of assists a faction has
        /// </summary>
        public int TotalAssists { get; set; }

        /// <summary>
        ///     total number of deaths a faction has gotten
        /// </summary>
        public int TotalDeaths { get; set; }

    }

    public class RealtimeDataFight {

        public RealtimeMapState MapState { get; set; } = new();

        public PsFacility? Facility { get; set; } = null;

    }

}
