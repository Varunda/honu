using System;
using System.Collections.Generic;
using System.Linq;
using watchtower.Constants;

namespace watchtower.Models {

    /// <summary>
    ///     Represents all the zones in a single world
    /// </summary>
    public class PsWorldMap {

        /// <summary>
        ///     ID of the world 
        /// </summary>
        public short WorldID { get; set; }

        private Dictionary<uint, PsZone> _Zones { get; set; } = new Dictionary<uint, PsZone>();

        /// <summary>
        ///     Get the <see cref="PsZone"/> given a zone ID
        /// </summary>
        /// <param name="zoneID">ID of the zone</param>
        /// <returns>
        ///     The <see cref="PsZone"/> with <see cref="PsZone.ZoneID"/> of <paramref name="zoneID"/>.
        ///     If no zone exists, one is created before this method returns
        /// </returns>
        public PsZone GetZone(uint zoneID) {
            if (_Zones.TryGetValue(zoneID, out PsZone? zone) == false) {
                zone = new PsZone();
                zone.ZoneID = zoneID;
                zone.WorldID = this.WorldID;

                // if the zone is nexus, set the default colors to make things more clear
                if ((zoneID & 0xFFFF) == Zone.Nexus) {
                    // TR starting bases (south)
                    zone.SetFacilityOwner(310570, 3); // warpgate alpha
                    zone.SetFacilityOwner(310540, 3); // argent
                    zone.SetFacilityOwner(310600, 3); // alpha
                    zone.SetFacilityOwner(310510, 3); // nexus secure

                    // NC starting bases (north)
                    zone.SetFacilityOwner(310560, 2); // warpgate omega
                    zone.SetFacilityOwner(310610, 2); // omega
                    zone.SetFacilityOwner(310520, 2); // hydro
                    zone.SetFacilityOwner(310550, 2); // slate

                    // neutrals, these stay as nothing
                    //zone.SetFacilityOwner(310500, 3); // arazek
                    //zone.SetFacilityOwner(310590, 3); // bitter gorge
                    //zone.SetFacilityOwner(310530, 3); // granitehead
                }

                _Zones.Add(zoneID, zone);
            }

            return zone;
        }
    }

    /// <summary>
    ///     Represents the facilities in a zone and who currently owns them
    /// </summary>
    public class PsZone {

        /// <summary>
        ///     ID of the zone
        /// </summary>
        public uint ZoneID { get; set; }

        public short WorldID { get; set; }

        public Dictionary<int, PsFacilityOwner> Facilities { get; set; } = new();

        /// <summary>
        ///     Get who currently owns a facility within this zone
        /// </summary>
        /// <param name="facID">ID of the facility</param>
        /// <returns>
        ///     Get the <see cref="PsFacilityOwner"/> for the facility, or null if the facility does not exist
        /// </returns>
        public PsFacilityOwner? GetFacilityOwner(int facID) {
            if (Facilities.TryGetValue(facID, out PsFacilityOwner? owner) == false) {
                return null;
            }
            return owner;
        }

        /// <summary>
        ///     Get a list of facilities and who owns each
        /// </summary>
        public List<PsFacilityOwner> GetFacilities() {
            return Facilities.Values.ToList();
        }

        /// <summary>
        ///     Set what faction owns a facility
        /// </summary>
        /// <param name="facID">ID of the facility</param>
        /// <param name="ownerID">team_id of the faction that now owns the facility</param>
        public void SetFacilityOwner(int facID, short ownerID) {
            if (Facilities.TryGetValue(facID, out PsFacilityOwner? owner) == false) {
                owner = new PsFacilityOwner() {
                    FacilityID = facID,
                    Owner = ownerID
                };

                Facilities.Add(facID, owner);
            }

            owner.Owner = ownerID;
        }

    }

    /// <summary>
    ///     Information about what faction currently owns a facility
    /// </summary>
    public class PsFacilityOwner {

        /// <summary>
        ///     ID of the facility
        /// </summary>
        public int FacilityID { get; set; }

        /// <summary>
        ///     ID of the faction/team that owns the facility
        /// </summary>
        public short Owner { get; set; }

    }

}
