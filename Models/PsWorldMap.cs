using System;
using System.Collections.Generic;
using System.Linq;

namespace watchtower.Models {

    public class PsWorldMap {

        public short WorldID { get; set; }

        private Dictionary<uint, PsZone> _Zones { get; set; } = new Dictionary<uint, PsZone>();

        public PsZone GetZone(uint zoneID) {
            if (_Zones.TryGetValue(zoneID, out PsZone? zone) == false) {
                zone = new PsZone();
                zone.ZoneID = zoneID;

                _Zones.Add(zoneID, zone);
            }

            return zone;
        }
    }

    public class PsZone {

        public uint ZoneID { get; set; }

        private Dictionary<int, PsFacilityOwner> _Faciltities { get; set; } = new Dictionary<int, PsFacilityOwner>();

        public PsFacilityOwner? GetFacilityOwner(int facID) {
            if (_Faciltities.TryGetValue(facID, out PsFacilityOwner? owner) == false) {
                return null;
            }
            return owner;
        }

        public List<PsFacilityOwner> GetFacilities() {
            return _Faciltities.Values.ToList();
        }

        public void SetFacilityOwner(int facID, short ownerID) {
            if (_Faciltities.TryGetValue(facID, out PsFacilityOwner? owner) == false) {
                owner = new PsFacilityOwner() {
                    FacilityID = facID,
                    Owner = ownerID
                };

                _Faciltities.Add(facID, owner);
            }

            owner.Owner = ownerID;
        }

    }

    public class PsFacilityOwner {

        public int FacilityID { get; set; }

        public short Owner { get; set; }

    }

}
