using System.Collections.Generic;

namespace watchtower.Models.Alert {

    public class AlertParticipantDataEntry {

        // Generic
        
        public long ID { get; set; }

        public long AlertID { get; set; }

        public string CharacterID { get; set; } = "";

        public string? OutfitID { get; set; } = null;

        public int SecondsOnline { get; set; }

        // Kills

        public int Kills { get; set; }

        public int Deaths { get; set; }

        public int VehicleKills { get; set; }

        // Support

        public int Heals { get; set; }

        public int Revives { get; set; }

        public int ShieldRepairs { get; set; }

        public int Resupplies { get; set; }

        public int Spawns { get; set; }

        public int Repairs { get; set; }

    }
}
