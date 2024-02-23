using System;
using System.Collections.Generic;
using watchtower.Constants;
using watchtower.Models.Census;

namespace watchtower.Models.Api {

    public class VehicleUsageData {

        public short WorldID { get; set; }

        public uint ZoneID { get; set; }

        public DateTime Timestamp { get; set; }

        public DateTime LastEvent { get; set; }

        public int Total { get; set; }

        public VehicleUsageFaction Vs { get; set; } = new(Faction.VS);
        public VehicleUsageFaction Nc { get; set; } = new(Faction.NC);
        public VehicleUsageFaction Tr { get; set; } = new(Faction.TR);
        public VehicleUsageFaction Other { get; set; } = new(0);

    }

    public class VehicleUsageFaction {

        public VehicleUsageFaction(int factionID) {
            this.FactionID = factionID;
        }

        public int FactionID { get; set; }

        public Dictionary<int, VehicleUsageEntry> Usage { get; set; } = new();

    }

    public class VehicleUsageEntry {

        public int VehicleID { get; set; }

        public PsVehicle? Vehicle { get; set; }

        public string VehicleName { get; set; } = "";

        public int Count { get; set; }

    }


}
