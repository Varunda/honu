using System;
using System.Collections.Generic;
using watchtower.Constants;
using watchtower.Models.Census;

namespace watchtower.Models.Api {

    public class VehicleUsageData {

        /// <summary>
        ///     ID of the DB entry this data is saved under. Is 
        /// </summary>
        public ulong ID { get; set; }

        /// <summary>
        ///     which world this data is for. a value of 0 means all worlds
        /// </summary>
        public short WorldID { get; set; }

        /// <summary>
        ///     which zone this data is for. a value of 0 means all zones
        /// </summary>
        public uint ZoneID { get; set; }

        /// <summary>
        ///     when this data was generated
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        ///     how many total characters were included in the summary
        /// </summary>
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

        /// <summary>
        ///     ID of the faction this usage is for
        /// </summary>
        public int FactionID { get; set; }

        /// <summary>
        ///     how many total players are in this faction summary
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        ///     how many total players are in vehicles for this faction
        /// </summary>
        public int TotalVehicles { get; set; }

        /// <summary>
        ///     the usage of each vehicle that is in use. if a vehicle is not in use, it will not be included in the map
        /// </summary>
        public Dictionary<int, VehicleUsageEntry> Usage { get; set; } = new();

    }

    public class VehicleUsageEntry {

        /// <summary>
        ///     id of the <see cref="PsVehicle"/>
        /// </summary>
        public int VehicleID { get; set; }

        /// <summary>
        ///     the vehicle this is for. can be null depending on how this data is generated
        /// </summary>
        public PsVehicle? Vehicle { get; set; }

        /// <summary>
        ///     name of the vehicle, or a missing string if the vehicle is unknown
        /// </summary>
        public string VehicleName { get; set; } = "";

        /// <summary>
        ///     how many characters are possibly in this vehicle
        /// </summary>
        public int Count { get; set; }

    }


}
