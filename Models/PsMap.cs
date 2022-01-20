using System.Collections.Generic;

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

        private Dictionary<int, short> _Faciltities { get; set; } = new Dictionary<int, short>();

        public short? GetFacilityOwner(int facID) {
            if (_Faciltities.TryGetValue(facID, out short owner) == false) {
                return null;
            }
            return owner;
        }

        public void SetFacilityOwner(int facID, short owner) {
            _Faciltities[facID] = owner;
        }

    }

}
